using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;
using ioschools.Library.Imaging;
using ioschools.Models.user.student;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.Helpers;
using ioschools.Library.Lucene;
using ioschools.Models;
using ioschools.Models.user;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ioschools.Controllers.users
{
    public class usersController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.USERS_CREATE)]
        public ActionResult Add()
        {
            var usr = new User();
            usr.designationList = typeof(Designation).ToSelectList(true, "", "");
            usr.schoolsList = typeof(Schools).ToSelectList(false, "select school", "");
            usr.maritalStatusList = typeof(MaritalStatus).ToSelectList(false, null, null, MaritalStatus.SINGLE.ToString());

            usr.dayList = DateHelper.GetDayList();
            usr.monthList = DateHelper.GetMonthList();
            usr.canModify = true;
            usr.canModifyStaff = auth.perms.HasFlag(Permission.USERS_EDIT_STAFF);
            var viewmodel = new UserViewModel(baseviewmodel);

            viewmodel.usr = usr;
            return View(viewmodel);
        }

        
        [HttpGet]
        public ActionResult Children(string term, long? sinceid)
        {
            var options = new IdName();
            options.id = sinceid.HasValue ? sinceid.Value.ToString() : "";
            options.name = term;
            return View(options);
        }

        public ActionResult ChildrenContent(string term, long? sinceid)
        {
            var usrs = repository.GetActiveUsers().Where(x => x.usergroup == (int)UserGroup.STUDENT);

            if (!string.IsNullOrEmpty(term))
            {
                var search = new LuceneSearch();
                var ids = search.UserSearch(term.ToLowerInvariant());
                usrs = usrs.Where(x => ids.Select(y => y.docId).Contains(x.id)).AsEnumerable();
                usrs = usrs.Join(ids, x => x.id, y => y.docId, (x, y) => new { x, y.score })
                    .OrderByDescending(x => x.score).Select(x => x.x);
            }

            if (sinceid.HasValue)
            {
                usrs = usrs.Where(x => x.id > sinceid);
            }
            usrs = usrs.OrderBy(x => x.id).Take(20);
            var viewmodel = usrs.ToBaseModel();
            return View(viewmodel);
        }
        
        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Delete(long id)
        {
            var canedit = false;
            var usr = repository.GetUser(id);
            if (usr != null)
            {
                canedit = usr.GetCanEdit(sessionid.Value, auth);
                
            }
            if (!canedit)
            {
                return SendJsonNoPermission();
            }

            try
            {
                repository.DeleteUser(id, sessionid.Value);

                LuceneUtil.DeleteLuceneIndex(id);

            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("User deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [NoCache]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Edit(long id)
        {
            var date = Utility.GetDBDate();
            var canedit = false;
            var usr = repository.GetUser(id);
            if (usr != null)
            {
                canedit = usr.GetCanEdit(sessionid.Value, auth);

            }
            if (!canedit)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new UserViewModel(baseviewmodel);
            viewmodel.usr = usr.ToModel(sessionid.Value, auth,  date.Year);
            viewmodel.usr.designationList = typeof(Designation).ToSelectList(true, "", "", usr.designation);
            viewmodel.usr.schoolsList = typeof (Schools).ToSelectList(false, "select school", "",
                                                                      viewmodel.usr.school.HasValue
                                                                          ? viewmodel.usr.school.Value.ToString()
                                                                          : "");

            if (viewmodel.usr.dob.HasValue)
            {
                viewmodel.usr.dayList = DateHelper.GetDayList(viewmodel.usr.dob.Value.Day);
                viewmodel.usr.monthList = DateHelper.GetMonthList(viewmodel.usr.dob.Value.Month);
            }

            return View("add", viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Export()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Export(bool dob, bool name, bool gender, bool guardian, bool sclass, bool ugroup, bool citizenship,
            bool contactnos, bool occupation, bool school, bool birthcert, bool icpassport, bool address, bool children, bool race, bool rank)
        {
            var searchcookie = Request.Cookies["search"];
            IEnumerable<user> results = Enumerable.Empty<user>();
            var search = new UserSearch();
            if (searchcookie != null)
            {
                var serializer = new JavaScriptSerializer();
                search = serializer.Deserialize<UserSearch>(HttpUtility.UrlDecode(searchcookie.Value));
                results = repository.GetUsers(sessionid.Value, auth,search.school, search.sclass, search.group, search.discipline, search.attStatus, search.date, search.year, search.seca, search.status ?? true);
                if (!string.IsNullOrEmpty(search.term))
                {
                    long userid;
                    if (long.TryParse(search.term, out userid))
                    {
                        results = repository.GetUsers().Where(x => x.id == userid);
                    }
                    else
                    {
                        var lsearch = new LuceneSearch();
                        var ids = lsearch.UserSearch(search.term.ToLowerInvariant());
                        results = results.Where(x => ids.Select(y => y.docId).Contains(x.id)).AsEnumerable();
                        results = results.Join(ids, x => x.id, y => y.docId, (x, y) => new { x, y.score })
                            .OrderByDescending(x => x.score).Select(x => x.x);
                    }

                }
            }

            var ms = new MemoryStream();
            using (var fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/NPOITemplate.xls",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var templateWorkbook = new HSSFWorkbook(fs, true);
                var rowcount = 0;
                var sheet = templateWorkbook.CreateSheet("Sheet1");
                var row = sheet.CreateRow(rowcount++);

                // bold font
                var boldStyle = templateWorkbook.CreateCellStyle();
                var fontBold = templateWorkbook.CreateFont();
                fontBold.IsBold = true;
                boldStyle.SetFont(fontBold);

                var colcount = 0;
                var totalcolumns = 0;

                // initialise heading
                row.CreateCell(colcount++).SetCellValue("No");
                if (name)
                {
                    row.CreateCell(colcount++).SetCellValue("Name");
                }
                if (ugroup)
                {
                    row.CreateCell(colcount++).SetCellValue("UserGroup");
                }
                if (race)
                {
                    row.CreateCell(colcount++).SetCellValue("Race");
                }
                if (gender)
                {
                    row.CreateCell(colcount++).SetCellValue("Gender");
                }
                if (dob)
                {
                    row.CreateCell(colcount++).SetCellValue("DOB");
                }
                if (school)
                {
                    row.CreateCell(colcount++).SetCellValue("School");
                }
                if (sclass)
                {
                    row.CreateCell(colcount++).SetCellValue("Class");
                }
                if (citizenship)
                {
                    row.CreateCell(colcount++).SetCellValue("Citizenship");
                }
                if (occupation)
                {
                    row.CreateCell(colcount++).SetCellValue("Father Occupation");
                    row.CreateCell(colcount++).SetCellValue("Mother Occupation");
                    row.CreateCell(colcount++).SetCellValue("Guardian Occupation");
                }
                if (contactnos)
                {
                    row.CreateCell(colcount++).SetCellValue("Contact Info");
                }
                if (birthcert)
                {
                    row.CreateCell(colcount++).SetCellValue("Birth Cert");
                }
                if (icpassport)
                {
                    row.CreateCell(colcount++).SetCellValue("NRIC");
                    row.CreateCell(colcount++).SetCellValue("Passport");
                }
                if (address)
                {
                    row.CreateCell(colcount++).SetCellValue("Address");
                }

                if (guardian)
                {
                    row.CreateCell(colcount++).SetCellValue("Father");
                    row.CreateCell(colcount++).SetCellValue("Father Contact");
                    row.CreateCell(colcount++).SetCellValue("Mother");
                    row.CreateCell(colcount++).SetCellValue("Mother Contact");
                    row.CreateCell(colcount++).SetCellValue("Guardian");
                    row.CreateCell(colcount++).SetCellValue("Guardian Contact");
                }

                if (children)
                {
                    row.CreateCell(colcount++).SetCellValue("Children Details");
                }

                if (rank)
                {
                    row.CreateCell(colcount++).SetCellValue("Rank");
                }

                totalcolumns = colcount;
                // now intialise data
                foreach (var usr in results.OrderBy(x => x.name))
                {
                    colcount = 0;
                    row = sheet.CreateRow(rowcount);
                    row.CreateCell(colcount++).SetCellValue(rowcount);
                    if (name)
                    {
                        row.CreateCell(colcount++).SetCellValue(SecurityElement.Escape(usr.ToName(false)));
                    }
                    if (ugroup)
                    {
                        row.CreateCell(colcount++).SetCellValue(((UserGroup)usr.usergroup).ToString());
                    }
                    if (race)
                    {
                        var usr_race = "";
                        if (!string.IsNullOrEmpty(usr.race))
                        {
                            usr_race = usr.race;
                        }
                        row.CreateCell(colcount++).SetCellValue(usr_race);
                    }
                    if (gender)
                    {
                        var usr_gender = "";
                        if (!string.IsNullOrEmpty(usr.gender))
                        {
                            usr_gender = usr.gender.Substring(0, 1);
                        }
                        row.CreateCell(colcount++).SetCellValue(usr_gender);
                    }
                    if (dob)
                    {
                        var usr_dob = "";
                        if (usr.dob.HasValue)
                        {
                            usr_dob = usr.dob.Value.ToShortDateString();
                        }
                        row.CreateCell(colcount++).SetCellValue(usr_dob);
                    }
                    if (school)
                    {
                        var usr_school = "";
                        if (search.school.HasValue)
                        {
                            usr_school = ((Schools) search.school.Value).ToString();
                        }
                        row.CreateCell(colcount++).SetCellValue(usr_school);
                    }
                    if (sclass)
                    {
                        var class_name = "";
                        if (usr.usergroup == (int)UserGroup.STUDENT)
                        {
                            var allocated = usr.classes_students_allocateds.SingleOrDefault(x => x.year == search.year);
                            if (allocated != null)
                            {
                                class_name = allocated.school_class.name;
                            }
                        }
                        else if (usr.usergroup == (int)UserGroup.TEACHER)
                        {
                            var allocated =
                                usr.classes_teachers_allocateds.Where(x => x.year == search.year);
                            if (allocated.Count() != 0)
                            {
                                class_name = string.Join(",", allocated.Select(x => x.school_class.name).ToArray());
                            }
                        }
                        row.CreateCell(colcount++).SetCellValue(SecurityElement.Escape(class_name));
                    }
                    if (citizenship)
                    {
                        var cship = "";
                        if (!string.IsNullOrEmpty(usr.citizenship))
                        {
                            cship = usr.citizenship;
                        }
                        row.CreateCell(colcount++).SetCellValue(cship);
                    }
                    if (occupation)
                    {
                        // father
                        var father_occ = 
                            usr.students_guardians.SingleOrDefault(x => x.type == GuardianType.FATHER.ToInt());

                        if (father_occ != null)
                        {
                            row.CreateCell(colcount++).SetCellValue(father_occ.user1.user_parents.occupation);
                        }
                        else
                        {
                            row.CreateCell(colcount++).SetCellValue("-");
                        }

                        // mother
                        var mother_occ =
                            usr.students_guardians.SingleOrDefault(x => x.type == GuardianType.MOTHER.ToInt());

                        if (mother_occ != null)
                        {
                            row.CreateCell(colcount++).SetCellValue(mother_occ.user1.user_parents.occupation);
                        }
                        else
                        {
                            row.CreateCell(colcount++).SetCellValue("-");
                        }

                        // guardian
                        var guardian_occ =
                            usr.students_guardians.SingleOrDefault(x => x.type == GuardianType.GUARDIAN.ToInt());

                        if (guardian_occ != null)
                        {
                            row.CreateCell(colcount++).SetCellValue(guardian_occ.user1.user_parents.occupation);
                        }
                        else
                        {
                            row.CreateCell(colcount++).SetCellValue("-");
                        }
                    }
                    if (contactnos)
                    {
                        row.CreateCell(colcount++).SetCellValue(SecurityElement.Escape(usr.ToContactString()));
                    }
                    if (birthcert)
                    {
                        var usr_birthcert = "";
                        if (!string.IsNullOrEmpty(usr.birthcertno))
                        {
                            usr_birthcert = usr.birthcertno;
                        }

                        row.CreateCell(colcount++).SetCellValue(usr_birthcert);
                    }
                    if (icpassport)
                    {
                        var usr_passport = "";
                        var usr_ic = "";
                        if (!string.IsNullOrEmpty(usr.nric_new))
                        {
                            usr_ic = usr.nric_new;
                        }
                        if (!string.IsNullOrEmpty(usr.passportno))
                        {
                            usr_passport = usr.passportno;
                        }

                        row.CreateCell(colcount++).SetCellValue(usr_ic);
                        row.CreateCell(colcount++).SetCellValue(usr_passport);
                    }
                    if (address)
                    {
                        var usr_address = "";
                        if (!string.IsNullOrEmpty(usr.address))
                        {
                            usr_address = usr.address;
                        }
                        row.CreateCell(colcount++).SetCellValue(usr_address);
                    }

                    if (guardian)
                    {
                        if (usr.usergroup == (int)UserGroup.STUDENT)
                        {
                            var fathername = row.CreateCell(colcount++);
                            var fathercontact = row.CreateCell(colcount++);
                            var mothername = row.CreateCell(colcount++);
                            var monthercontact = row.CreateCell(colcount++);
                            var guardianname = row.CreateCell(colcount++);
                            var guardiancontact = row.CreateCell(colcount++);

                            var currentfather =
                                usr.students_guardians.Where(
                                    x => x.type == (int)GuardianType.FATHER).Select(x => x.user1)
                                    .FirstOrDefault();
                            var currentmother = usr.students_guardians.Where(
                                                            x => x.type == (int)GuardianType.MOTHER).Select(x => x.user1)
                                                            .FirstOrDefault();
                            var currentguardian = usr.students_guardians.Where(
                                                            x => x.type == (int)GuardianType.GUARDIAN).Select(x => x.user1)
                                                            .FirstOrDefault();

                            if (currentfather != null)
                            {
                                fathername.SetCellValue(currentfather.ToName(false));
                                fathercontact.SetCellValue(currentfather.ToContactString());
                            }
                            else
                            {
                                fathername.SetCellValue("-");
                                fathercontact.SetCellValue("-");
                            }

                            if (currentmother != null)
                            {
                                mothername.SetCellValue(currentmother.ToName(false));
                                monthercontact.SetCellValue(currentmother.ToContactString());
                            }
                            else
                            {
                                mothername.SetCellValue("-");
                                monthercontact.SetCellValue("-");
                            }

                            if (currentguardian != null)
                            {
                                guardianname.SetCellValue(currentguardian.ToName(false));
                                guardiancontact.SetCellValue(currentguardian.ToContactString());
                            }
                            else
                            {
                                guardianname.SetCellValue("-");
                                guardiancontact.SetCellValue("-");
                            }
                        }
                    }

                    if (children)
                    {
                        string childrenString = "";
                        if (usr.usergroup == (int)UserGroup.GUARDIAN)
                        {
                            var childrenlist =
                                usr.students_guardians1
                                .ToChildrenModel(search.year)
                                .Select(x => string.Format("{0} {1}", x.name, !string.IsNullOrEmpty(x.class_name)?string.Format("{0}",x.class_name):"" )).
                                    ToArray();
                            childrenString = string.Join(", ", childrenlist);
                        }
                        row.CreateCell(colcount++).SetCellValue(SecurityElement.Escape(childrenString));
                    }

                    if (rank)
                    {
                        string rankValue = "";
                        if (usr.usergroup == (int)UserGroup.STUDENT)
                        {
                            rankValue = usr.ToRankStudent();
                        }
                        row.CreateCell(colcount++).SetCellValue(rankValue);
                    }

                    rowcount++;
                }

                // do some formatting
                row = sheet.GetRow(0);
                for (int i = 0; i < totalcolumns; i++)
                {
                    var cell = row.GetCell(i);
                    cell.CellStyle = boldStyle;
                    sheet.AutoSizeColumn(i);
                }
                
                // delete first sheet
                templateWorkbook.RemoveSheetAt(0);
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("UserExport_{0}.xls", DateTime.Now.ToShortDateString().Replace("/", "")));
        }

        [GroupsFilter(group = UserSuperGroups.SUPERSTAFF)]
        public ActionResult Issues(long id)
        {
            var issues = repository.GetUser(id).GetIssues(DateTime.Now.Year, false);

            if (issues == UserIssue.NONE)
            {
                return Content("");
            }

            var viewmodel = issues.ToString()
                .Split(new[] {","}, StringSplitOptions.None)
                .Select(x => x.ToEnum<UserIssue>().ToDescriptionString());

            return View(viewmodel);
        }

        [GroupsFilter(group = UserSuperGroups.SUPERSTAFF)]
        public ActionResult IssuesFilter()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult List(int? school, int? form, UserGroup? group, int rows, int page, int? year,
            string term, string discipline, AttendanceStatus? attendanceStatus, string attendanceDate, bool? active, string sord, string sidx,
            int? eca, bool? hasIssues)
        {
            IEnumerable<user> results;

            // year will be ignored unless school or class is present
            if (!year.HasValue)
            {
                year = Utility.GetDBDate().Year;
            }

            if (!string.IsNullOrEmpty(term))
            {
                long userid;
                if (long.TryParse(term, out userid))
                {
                    results = repository.GetUsers().Where(x => x.id == userid);
                }
                else
                {
                    results = repository.GetUsers();
                    var search = new LuceneSearch();
                    var ids = search.UserSearch(term.ToLowerInvariant().ToLiteral());
                    results = results.Where(x => ids.Select(y => y.docId).Contains(x.id)).AsEnumerable();
                    results = results.Join(ids, x => x.id, y => y.docId, (x, y) => new { x, y.score })
                        .OrderByDescending(x => x.score).Select(x => x.x);
                    
                    // limit results
                    results = results.Where(x => x.GetCanView(sessionid.Value, auth));
                }
            }
            else
            {
                results = repository.GetUsers(sessionid.Value, auth, school, form, group, discipline, attendanceStatus, attendanceDate, year.Value, eca, active ?? true, hasIssues ?? false);
            
                if (sord == "asc")
                {
                    switch (sidx)
                    {
                        case "name":
                            results = results.OrderBy(x => x.name);
                            break;
                        default:
                            results = results.OrderBy(x => x.id);
                            break;
                    }
                }
                else
                {
                    switch (sidx)
                    {
                        case "name":
                            results = results.OrderByDescending(x => x.name);
                            break;
                        default:
                            results = results.OrderByDescending(x => x.id);
                            break;
                    }
                }
            }

            var contacts = results.Skip(rows * (page - 1)).Take(rows).ToUsersJqGrid(year.Value);
            var records = results.Count();
            var total = (records / rows);
            if (records % rows != 0)
            {
                total++;
            }

            contacts.page = page;
            contacts.records = records;
            contacts.total = total;

            return Json(contacts);
        }

        [HttpGet]
        public ActionResult Parents(string term, long? sinceid)
        {
            var options = new IdName();
            options.id = sinceid.HasValue?sinceid.Value.ToString():"";
            options.name = term;
            return View(options);
        }

        public ActionResult ParentsContent(string term, long? sinceid)
        {
            var usrs = repository.GetActiveUsers().Where(x => x.usergroup == (int)UserGroup.GUARDIAN);
            
            if (!string.IsNullOrEmpty(term))
            {
                var search = new LuceneSearch();
                var ids = search.UserSearch(term.ToLowerInvariant());
                usrs = usrs.Where(x => ids.Select(y => y.docId).Contains(x.id)).AsEnumerable();
                usrs = usrs.Join(ids, x => x.id, y => y.docId, (x, y) => new { x, y.score })
                    .OrderByDescending(x => x.score).Select(x => x.x);
            }

            if (sinceid.HasValue)
            {
                usrs = usrs.Where(x => x.id > sinceid);
            }
            usrs = usrs.OrderBy(x => x.id).Take(20);
            var viewmodel = usrs.ToBaseModel();
            return View(viewmodel);
        }
        [HttpPost]
        [PermissionFilter(perm = Permission.USERS_EDIT_STAFF)]
        public ActionResult Permissions(long id, string name, bool perm)
        {
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return Json("User not found".ToJsonFail());
            }

            var p = name.ToEnum<Permission>();

            if (perm)
            {
                usr.permissions |= (long)p;
            }
            else
            {
                usr.permissions &= ~((long) p);
            }

            repository.Save();

            return Json("Permission updated".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Save(long? id, string designation, string name, string email, Schools? uschool,
            int[] day, int[] year, int?[] school, int?[] schoolclass, string[] subject, 
            long[] parent, int[] parentrel, long[] child, int[] childrel,
            UserGroup? ugroup, long? thumbnailid, int[] start_hour, int[] start_minutes, int[] end_hour, int[] end_minutes,
            string race, string dialect, int dob_day, int dob_month, int? dob_year,
            string pob, string citizenship, string birthcertno, string passport, bool bumi, string nric_new,
            string homephone, string cellphone, string address, string religion, Gender gender, MaritalStatus marital_status, 
            string occupation, string officephone, string employer, string notes,
            // staff stuff
            string staff_socso, string staff_salary_grade, string staff_epf, string staff_income_tax,
            string staff_spouse_phone_cell, string staff_spouse_phone_office, string staff_spouse_employer_address, 
            string staff_spouse_employer, string staff_spouse_name
            )
        {
            if (email == null)
            {
                email = "";
            }
            email = email.Trim().ToLower();

            // TODO check that staff / student id is unique
            var emailchanged = true;
            var u = new user();
            if (id.HasValue)
            {
                u = repository.GetUser(id.Value);
                if (u == null)
                {
                    return Json("Unable to find user".ToJsonFail());
                }
                if (u.email == email)
                {
                    emailchanged = false;
                }
            }
            else
            {
                // can we create new user?
                if (!auth.perms.HasFlag(Permission.USERS_CREATE))
                {
                    return SendJsonNoPermission();
                }
                
                // dont allow change of usergroups for the moment because there are specific actions 
                // that need to be performed when a certain type of user is added
                // only set when user is created
                if (ugroup.HasValue)
                {
                    u.usergroup = (int)ugroup.Value;
                    u.permissions = (long) UserHelper.GetDefaultPermission(ugroup.Value);
                }
                u.settings = (int)UserSettings.NONE;
            }

            // check that email is unique
            if (!string.IsNullOrEmpty(email))
            {
                var duplicate = repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, email) == 0);
                if (duplicate != null && duplicate.id != u.id)
                {
                    return Json("Email address is already in use".ToJsonFail());
                }
            }

            // check that nric is unique
            if (!string.IsNullOrEmpty(nric_new))
            {
                var duplicate = repository.GetUsers().FirstOrDefault(x => string.Compare(x.nric_new, nric_new) == 0);
                if (duplicate != null && duplicate.id != u.id)
                {
                    return Json("NRIC is already in use".ToJsonFail());
                }
            }

            if (uschool.HasValue)
            {
                u.schoolid = uschool.Value.ToInt();
            }

            u.gender = gender.ToString();
            u.designation = designation;
            u.name = name;
            u.email = email;
            u.photo = thumbnailid;
            u.race = race;
            u.dialect = dialect;
            if (dob_year.HasValue)
            {
                try
                {
                    u.dob = new DateTime(dob_year.Value, dob_month, dob_day);
                }
                catch
                {
                    return Json("Invalid Date of Birth".ToJsonFail());
                }
            }
            u.pob = pob;
            u.citizenship = citizenship;
            u.birthcertno = birthcertno;
            u.passportno = passport;
            u.isbumi = bumi;
            u.nric_new = nric_new;
            u.phone_home = homephone;
            u.phone_cell = cellphone;
            u.address = address;
            u.religion = religion;
            u.notes = notes;
            u.marital_status = marital_status.ToString();

            if (!ugroup.HasValue)
            {
                ugroup = (UserGroup) u.usergroup;
            }

            switch (ugroup)
            {
                case UserGroup.GUARDIAN:
                    if (u.user_parents == null)
                    {
                        u.user_parents = new user_parent();
                    }
                    if (!string.IsNullOrEmpty(employer))
                    {
                        employer = employer.Trim();
                    }
                    u.user_parents.employer = employer;
                    u.user_parents.phone_office = officephone;
                    u.user_parents.occupation = occupation;

                    if (child != null)
                    {
                        for (int i = 0; i < child.Length; i++)
                        {
                            var student = new students_guardian();
                            student.studentid = child[i];
                            student.type = Convert.ToByte(childrel[i]);
                            u.students_guardians1.Add(student);
                        }
                    }
                    break;
                case UserGroup.HEAD:
                case UserGroup.TEACHER:
                    if (schoolclass != null)
                    {
                        for (int i = 0; i < schoolclass.Length; i++)
                        {
                            var assigned = new classes_teachers_allocated();
                            assigned.day = day[i];
                            assigned.year = year[i];
                            if (school[i] == null)
                            {
                                return Json("School not specified".ToJsonFail());
                            }
                            assigned.schoolid = school[i].Value;
                            if (schoolclass[i] == null)
                            {
                                return Json("Class is not specified".ToJsonFail());
                            }
                            assigned.classid = schoolclass[i].Value;

                            // allow NULL subject for kindy classes as they don't have subjects
                            if (subject != null && !string.IsNullOrEmpty(subject[i]))
                            {
                                assigned.subjectid = long.Parse(subject[i]);
                            }
                            
                            assigned.time_start = new TimeSpan(start_hour[i], start_minutes[i], 0);
                            assigned.time_end = new TimeSpan(end_hour[i], end_minutes[i], 0);

                            // check that period is not already assigned
                            var period = repository.GetClassPeriod(assigned.year, assigned.day, assigned.schoolid, assigned.classid,
                                                      assigned.time_start, assigned.time_end);
                            if (period != null)
                            {
                                // only give warning if class allocated is owner's own as we want to allow 
                                // assistants to share the same period
                                if (id.HasValue && period.teacherid == id.Value)
                                {
                                    return
                                    Json(
                                        string.Format(
                                            "A class from {0} to {1} has already been assigned to {2} for {3}",
                                            period.time_start,
                                            period.time_end,
                                            period.user.ToName(),
                                            period.subject == null ? "" : period.subject.name).
                                            ToJsonFail());
                                }
                            }
                            u.classes_teachers_allocateds.Add(assigned);

                        }
                    }
                    break;
                case UserGroup.STUDENT:
                    if (schoolclass != null)
                    {
                        for (int i = 0; i < schoolclass.Length; i++)
                        {
                            var assigned = new classes_students_allocated();
                            assigned.year = year[i];
                            if (schoolclass[i] == null)
                            {
                                return Json("Class is not specified".ToJsonFail());
                            }
                            assigned.classid = schoolclass[i].Value;

                            // check that class is not already assigned
                            var exist =
                                u.classes_students_allocateds.SingleOrDefault(x => x.year == assigned.year);
                            if (exist == null)
                            {
                                u.classes_students_allocateds.Add(assigned);
                            }
                            else
                            {
                                return Json(string.Format("A class for the year {0} has already been allocated.", exist.year).ToJsonFail());
                            }
                            
                        }
                    }
                    if (parent != null)
                    {
                        for (int i = 0; i < parent.Length; i++)
                        {
                            var guardian = new students_guardian();
                            guardian.parentid = parent[i];
                            guardian.type = Convert.ToByte(parentrel[i]);
                            u.students_guardians.Add(guardian);
                        }

                        // validate not more than 1 mother or father
                        if (u.students_guardians.Count(x => x.type.HasValue && x.type == GuardianType.FATHER.ToInt()) > 1)
                        {
                            return Json("Cannot add more than 1 father".ToJsonFail());
                        }

                        if (u.students_guardians.Count(x => x.type.HasValue && x.type == GuardianType.MOTHER.ToInt()) > 1)
                        {
                            return Json("Cannot add more than 1 mother".ToJsonFail());
                        }

                        if (u.students_guardians.Count(x => x.type.HasValue && x.type == GuardianType.GUARDIAN.ToInt()) > 1)
                        {
                            return Json("Cannot add more than 1 guardian".ToJsonFail());
                        }
                    }
                    break;
            } // end switch

            // do STAFF only actions
            if (UserSuperGroups.STAFF.HasFlag(ugroup.Value) && 
                UserSuperGroups.SUPERSTAFF.HasFlag(auth.group))
            {
                if (u.user_staffs == null)
                {
                    u.user_staffs = new user_staff();
                }
                u.user_staffs.socso = staff_socso;
                u.user_staffs.salary_grade = staff_salary_grade;
                u.user_staffs.epf = staff_epf;
                u.user_staffs.income_tax = staff_income_tax;
                u.user_staffs.spouse_phone_cell = staff_spouse_phone_cell;
                u.user_staffs.spouse_phone_work = staff_spouse_phone_office;
                u.user_staffs.spouse_employer_address = staff_spouse_employer_address;
                u.user_staffs.spouse_employer = staff_spouse_employer;
                u.user_staffs.spouse_name = staff_spouse_name;
            }

            // check if we can actually edit
            var canedit = u.GetCanEdit(sessionid.Value, auth);

            if (!canedit)
            {
                return SendJsonNoPermission();
            }

            if (!id.HasValue)
            {
                repository.AddUser(u);
            }

            // log changes
            EntityLogging.LogChanges(db,u, u.name, sessionid.Value);

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            // try to update school
            if (!u.schoolid.HasValue)
            {
                u.schoolid = u.GetNewSchoolID();
            }
            repository.Save();
            
            // resend password email if email has been changed OR a user has been created
            if (emailchanged && !string.IsNullOrEmpty(email))
            {
                var password = clearpixels.crypto.Utility.GetRandomString(uppercase: true);
                var hash = Utility.GeneratePasswordHash(email, password);
                u.passwordhash = hash;
                u.settings = u.SetFlag(UserSettings.PASSWORD_RESET);
                repository.Save();
                var credentials = new UserCredentials { password = password, email = email };
                this.SendEmailNow(EmailViewType.PASSWORD_RESET, credentials, "New Account Password", email, u.ToName());
            }

            LuceneUtil.UpdateLuceneIndex(u);

            var jsonmodel = "User successfully saved".ToJsonOKMessage();
            jsonmodel.data = u.id;

            return Json(jsonmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult SelectableBlock(long id)
        {
            var viewmodel = repository.GetUser(id).ToBaseModel();
            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Single(long id)
        {
            var date = Utility.GetDBDate();
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return ReturnNotFoundView();
            }

            var canview = usr.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }
            var viewmodel = new UserViewModel(baseviewmodel);
            viewmodel.usr = usr.ToModel(sessionid.Value, auth, date.Year);
            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult StaffSelect(string term, long? sinceid)
        {
            var options = new IdName();
            options.id = sinceid.HasValue ? sinceid.Value.ToString() : "";
            options.name = term;
            return View(options);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult StaffSelectContent(string term, long? sinceid)
        {
            var usrs = repository.GetActiveUsers().Where(x => x.usergroup != (int)UserGroup.STUDENT && x.usergroup != (int)UserGroup.GUARDIAN);

            if (!string.IsNullOrEmpty(term))
            {
                var search = new LuceneSearch();
                var ids = search.UserSearch(term.ToLowerInvariant());
                usrs = usrs.Where(x => ids.Select(y => y.docId).Contains(x.id)).AsEnumerable();
                usrs = usrs.Join(ids, x => x.id, y => y.docId, (x, y) => new { x, y.score })
                    .OrderByDescending(x => x.score).Select(x => x.x);
            }

            if (sinceid.HasValue)
            {
                usrs = usrs.Where(x => x.id > sinceid);
            }
            usrs = usrs.OrderBy(x => x.id).Take(20);
            var viewmodel = usrs.ToBaseModel();
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Students(int id, int? year, bool? active)
        {
            if (year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var students = db.classes_students_allocateds
                .Where(x => x.classid == id && x.year == year);

            if (active.HasValue)
            {
                if (active.Value)
                {
                    students = students.Where(x => (x.user.settings & (int)UserSettings.INACTIVE) == 0);
                }
                else
                {
                     students = students.Where(x => (x.user.settings & (int)UserSettings.INACTIVE) != 0);
                }
            }

            var data = students
                .OrderBy(x => x.user.name)
                .ToArray()
                .Select(x => new StudentJSON()
                                 {
                                     classname = x.school_class.name,
                                     classid = x.classid,
                                     id = x.studentid.ToString(),
                                     name = x.user.ToName(false),
                                     imageUrl = x.user.photo.HasValue
                                                 ? Img.by_size(x.user.user_image.url, Imgsize.USER_THUMB)
                                                 : Img.PHOTO_NO_THUMBNAIL,
                                 });

            return Json(data.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult StudentSelector()
        {
            return View();
        }
    }
}
