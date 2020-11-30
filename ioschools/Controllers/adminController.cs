using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.Models;
using ioschools.Models.eca;
using ioschools.Models.leave;
using ioschools.Models.school;
using ioschools.Models.school.json;
using ioschools.Models.subject;
using ioschools.Models.subject.viewmodels;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.scheduler;
using ioschools.Models.admin;
using ioschools.Models.user;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ioschools.Controllers
{
    [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
    public class adminController : baseController
    {
        private enum IDTYPE
        {
            PASSPORT,
            NEWIC,
            BIRTHCERT
        }

        public ActionResult Index()
        {
            var viewmodel = new AdminViewModel(baseviewmodel)
            {
                cacheTimer1Min = HttpRuntime.Cache[CacheTimerType.Minute1.ToString()] != null,
                cacheTimer5Min = HttpRuntime.Cache[CacheTimerType.Minute5.ToString()] != null,
                cacheTimer10Min = HttpRuntime.Cache[CacheTimerType.Minute10.ToString()] != null,
                cacheTimer60Min = HttpRuntime.Cache[CacheTimerType.Minute60.ToString()] != null,
                mailQueueLength = repository.GetMails().Count(),
            };

            viewmodel.changeLogYears =
                repository.GetChangeLogs().Select(x => x.created.Year)
                    .Distinct()
                    .Select(y => new SelectListItem() { Text = y.ToString(), Value = y.ToString() })
                    .ToList();

            // enrolment notifiers
            viewmodel.enrolmentNotifiers = repository.GetRegistrationNotifications().Select(x => x.user).ToBaseModel();


            return View(viewmodel);
        }

        public ActionResult ChangeLog(int? log_from_day, int? log_from_month, int? log_from_year, int? log_to_day, int? log_to_month, int? log_to_year)
        {
            var changelogs = repository.GetChangeLogs();

            if (log_from_day.HasValue && log_from_month.HasValue && log_from_year.HasValue)
            {
                var from = new DateTime(log_from_year.Value, log_from_month.Value, log_from_day.Value);
                changelogs = changelogs.Where(x => x.created > from);
            }

            if (log_to_day.HasValue && log_to_month.HasValue && log_to_year.HasValue)
            {
                var to = new DateTime(log_to_year.Value, log_to_month.Value, log_to_day.Value);
                changelogs = changelogs.Where(x => x.created < to);
            }

            var viewmodel = new ChangeLogViewModel()
                                {
                                    changelogs = changelogs.OrderByDescending(x => x.id).Take(50).ToModel(),
                                    total = changelogs.Count()
                                };

            return View(viewmodel);
        }

        public ActionResult ChangeLogContent(long id)
        {
            var viewmodel = repository.GetChangeLogs().Where(x => x.id < id)
                                    .OrderByDescending(x => x.id)
                                    .Take(50)
                                    .ToModel();
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult Classes()
        {
            var viewmodel =
                new[] { new SelectListItem() { Text = "Select school ...", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.EXAM_ADMIN)]
        public ActionResult Grading()
        {
            return View();
        }

        [PermissionFilter(perm = Permission.ECA_ADMIN)]
        public ActionResult Eca()
        {
            var viewmodel = new ECAViewModel();
            viewmodel.schools = new[] { new SelectListItem() { Text = "All Schools", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));
            viewmodel.ecas = repository.GetViewableEcas(null).OrderBy(x => x.schoolid).ThenBy(x => x.name).ToModel();

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ECA_ADMIN)]
        public ActionResult EcaContent(int? id)
        {
            var viewmodel = repository.GetEcas(id).OrderBy(x => x.schoolid).ThenBy(x => x.name).ToModel();
            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult RolloverStudents(int id)
        {
            var currentYear = id - 1;
            var students = repository.GetUsers(null, null, null, null, UserGroup.STUDENT, null, null, null, currentYear, null);
            var noclass = 0;
            var noclassyear = 0;
            var alreadyalloc = 0;
            foreach (var student in students)
            {
                if (student.classes_students_allocateds.Count == 0)
                {
                    noclass++;
                    continue;
                }
                // get current allocated class
                var allocated = student.classes_students_allocateds.SingleOrDefault(x => x.year == currentYear);
                if (allocated == null)
                {
                    noclassyear++;
                    continue;
                }

                // check that nothing already allocated
                var checkallocated = student.classes_students_allocateds.SingleOrDefault(x => x.year == id);
                if (checkallocated != null)
                {
                    alreadyalloc++;
                    continue;
                }

                // get next class
                var nextClass = allocated.school_class.nextclass;

                // allocate class
                if (nextClass.HasValue)
                {
                    var newalloc = new classes_students_allocated();
                    newalloc.classid = nextClass.Value;
                    newalloc.studentid = student.id;
                    newalloc.year = id;
                    student.classes_students_allocateds.Add(newalloc);

                    // set school id
                    student.schoolid = student.GetNewSchoolID();
                }
            }

            Syslog.Write(ErrorLevel.INFORMATION, string.Format("STUDENTROLLOVER:{0}:NoAllocatedClasses:{1} NoAllocatedClassesForYear:{2} AlreadyAllocated:{3}", sessionid.Value, noclass, noclassyear, alreadyalloc));

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Students have been rolled over to the next year".ToJsonOKMessage());
        }

        [HttpGet]
        public ActionResult TermsContent(int year)
        {
            var viewmodel = new List<SchoolTermsViewModel>();

            foreach (var entry in db.schools)
            {
                var school = entry;
                var schoolTerms = new List<SchoolTerm>();
                var terms = repository.GetSchoolTerms().Where(x => x.schoolid == school.id);
                foreach (var term in terms)
                {
                    var att = term.attendance_terms.SingleOrDefault(x => x.year == year);
                    SchoolTerm row;
                    if (att == null)
                    {
                        row = new SchoolTerm { term = term.name, termid = term.id, year = year };
                    }
                    else
                    {
                        row = att.ToModel();
                    }
                    schoolTerms.Add(row);
                }
                var model = new SchoolTermsViewModel
                {
                    terms = schoolTerms,
                    schoolid = school.id,
                    schoolname = school.name
                };
                viewmodel.Add(model);
            }
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult Subjects()
        {
            var viewmodel =
                new[] { new SelectListItem() { Text = "All Schools", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        public ActionResult SubjectContent(int? id)
        {
            var viewmodel = repository.GetSchoolSubjects(id).OrderBy(x => x.schoolid).ThenBy(x => x.name).ToModel();
            return View(viewmodel);
        }

        public ActionResult Conduct()
        {
            return View();
        }

        public ActionResult CheckSchoolid(bool? commit)
        {
            var ids = new List<long>();

            foreach (var stu in db.users.Where(x => x.usergroup == UserGroup.STUDENT.ToInt()))
            {
                var newid = stu.GetNewSchoolID();
                if (newid != stu.schoolid)
                {
                    ids.Add(stu.id);
                    stu.schoolid = newid;
                }
            }

            if (commit.HasValue && commit.Value)
            {
                repository.Save();
            }

            return Content(string.Join(", ", ids));
        }

#if DEBUG
        public ActionResult RemoveEmails()
        {
            foreach (var usr in db.users)
            {
                usr.email = "";
            }
            repository.Save();
            return Content("done");
        }
        
        private ActionResult ImportStudent(bool? commit)
        {
            var fs = System.IO.File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/Content/media/student.xls");
            HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
            var sheet = templateWorkbook.GetSheet("student");
            var count = 1;  // skips header

            try
            {
                while (true)
                {
                    var row = sheet.GetRow(count++);
                    if (row == null)
                    {
                        break;
                    }

                    var name = GetCellValueAsString(row.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK));

                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }

                    var leavingDate = GetCellValueAsString(row.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var leavingState = GetCellValueAsString(row.GetCell(3, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var currentYear = GetCellValueAsString(row.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var dob = GetCellValueAsString(row.GetCell(5, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var nricpassport = GetCellValueAsString(row.GetCell(6, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var admissiondate = GetCellValueAsString(row.GetCell(7, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var schoolclass = GetCellValueAsString(row.GetCell(11, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var rank = GetCellValueAsString(row.GetCell(12, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var sex = GetCellValueAsString(row.GetCell(13, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var race = GetCellValueAsString(row.GetCell(14, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var nationality = GetCellValueAsString(row.GetCell(15, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var religion = GetCellValueAsString(row.GetCell(16, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var phone_house = GetCellValueAsString(row.GetCell(17, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var father_name = GetCellValueAsString(row.GetCell(18, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var father_nricpassport = GetCellValueAsString(row.GetCell(19, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var father_phone_cell = GetCellValueAsString(row.GetCell(20, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var father_phone_office = GetCellValueAsString(row.GetCell(21, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var father_occupation = GetCellValueAsString(row.GetCell(22, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var mother_name = GetCellValueAsString(row.GetCell(23, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var mother_nricpassport = GetCellValueAsString(row.GetCell(24, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var mother_phone_cell = GetCellValueAsString(row.GetCell(25, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var mother_phone_office = GetCellValueAsString(row.GetCell(26, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var mother_occupation = GetCellValueAsString(row.GetCell(27, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var address1 = GetCellValueAsString(row.GetCell(28, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var address2 = GetCellValueAsString(row.GetCell(29, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var postcode = GetCellValueAsString(row.GetCell(30, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var guardian_name = GetCellValueAsString(row.GetCell(31, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var guardian_nricpassport = GetCellValueAsString(row.GetCell(32, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var guardian_phone_hand = GetCellValueAsString(row.GetCell(33, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                    var guardian_phone_home = GetCellValueAsString(row.GetCell(34, MissingCellPolicy.RETURN_NULL_AND_BLANK));

                    var student = new user();
                    student.designation = "";
                    student.usergroup = (int)UserGroup.STUDENT;
                    student.name = name;
                    if (leavingState == "0")
                    {
                        student.settings = (int)UserSettings.INACTIVE;
                    }
                    else
                    {
                        student.settings = (int)UserSettings.NONE;
                    }
                    if (!string.IsNullOrEmpty(sex))
                    {
                        if (sex == "M")
                        {
                            student.gender = Gender.MALE.ToString();
                        }
                        else if (sex == "F")
                        {
                            student.gender = Gender.FEMALE.ToString();
                        }
                        else
                        {
                            return Content("Unrecognised gender row " + count);
                        }
                    }

                    if (!string.IsNullOrEmpty(race))
                    {
                        if (race == "C")
                        {
                            student.race = "Chinese";
                        }
                        else
                        {
                            student.race = race;
                        }
                    }
                    if (!string.IsNullOrEmpty(nationality))
                    {
                        student.citizenship = nationality;
                    }
                    if (!string.IsNullOrEmpty(religion))
                    {
                        student.religion = religion;
                    }
                    student.dob = ParseDate(dob);
                    if (!string.IsNullOrEmpty(nricpassport))
                    {
                        switch (GetIDType(nricpassport))
                        {
                            case IDTYPE.PASSPORT:
                                student.passportno = nricpassport;
                                break;
                            case IDTYPE.NEWIC:
                                student.nric_new = nricpassport;
                                break;
                            case IDTYPE.BIRTHCERT:
                                student.birthcertno = nricpassport;
                                break;
                        }
                    }

                    var registration = new registration();
                    registration.admissionDate = ParseDate(admissiondate);
                    registration.leftDate = ParseDate(leavingDate);
                    student.registrations.Add(registration);

                    if (!string.IsNullOrEmpty(schoolclass))
                    {
                        var school_class = repository.GetSchoolClasses().SingleOrDefault(x => x.name == schoolclass);
                        if (school_class == null)
                        {
                            return Content("unrecognised school: row " + count);
                        }
                        var student_class_allocated = new classes_students_allocated();
                        student_class_allocated.classid = school_class.id;
                        if (string.IsNullOrEmpty(currentYear))
                        {
                            student_class_allocated.year = 2011;
                        }
                        else
                        {
                            student_class_allocated.year = int.Parse(currentYear);
                        }
                        student.classes_students_allocateds.Add(student_class_allocated);
                    }

                    repository.AddUser(student);

                    var address = string.Concat(address1, Environment.NewLine, address2, Environment.NewLine, postcode);

                    user father = null;
                    if (!string.IsNullOrEmpty(father_name))
                    {
                        father = new user();
                        bool foundDuplicate = false;
                        if (!string.IsNullOrEmpty(father_nricpassport))
                        {
                            user found = null;
                            switch (GetIDType(father_nricpassport))
                            {
                                case IDTYPE.PASSPORT:
                                    father.passportno = father_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.passportno == father_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        father = found;
                                    }
                                    break;
                                case IDTYPE.NEWIC:
                                    father.nric_new = father_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.nric_new == father_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        father = found;
                                    }
                                    break;
                                case IDTYPE.BIRTHCERT:
                                    father.birthcertno = father_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.birthcertno == father_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        father = found;
                                    }
                                    break;
                            }
                        }
                        if (!foundDuplicate)
                        {
                            father.designation = "";
                            father.usergroup = (int)UserGroup.GUARDIAN;
                            father.user_parents = new user_parent();
                            father.settings = (int)UserSettings.NONE;
                            father.gender = Gender.MALE.ToString();
                            father.name = father_name;
                            father.address = address;
                            if (!string.IsNullOrEmpty(phone_house))
                            {
                                father.phone_home = phone_house;
                            }
                            if (!string.IsNullOrEmpty(father_phone_cell))
                            {
                                father.phone_cell = father_phone_cell;
                            }
                            if (!string.IsNullOrEmpty(father_phone_office))
                            {
                                father.user_parents.phone_office = father_phone_office;
                            }
                            if (!string.IsNullOrEmpty(father_occupation))
                            {
                                father.user_parents.occupation = father_occupation;
                            }
                            repository.AddUser(father);
                        }
                    }

                    user mother = null;
                    if (!string.IsNullOrEmpty(mother_name))
                    {
                        mother = new user();
                        bool foundDuplicate = false;
                        if (!string.IsNullOrEmpty(mother_nricpassport))
                        {
                            user found = null;
                            switch (GetIDType(mother_nricpassport))
                            {
                                case IDTYPE.PASSPORT:
                                    mother.passportno = mother_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.passportno == mother_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        mother = found;
                                    }
                                    break;
                                case IDTYPE.NEWIC:
                                    mother.nric_new = mother_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.nric_new == mother_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        mother = found;
                                    }
                                    break;
                                case IDTYPE.BIRTHCERT:
                                    mother.birthcertno = mother_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.birthcertno == mother_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        mother = found;
                                    }
                                    break;
                            }
                        }
                        if(!foundDuplicate)
                        {
                            mother.designation = "";
                            mother.usergroup = (int)UserGroup.GUARDIAN;
                            mother.user_parents = new user_parent();
                            mother.settings = (int)UserSettings.NONE;
                            mother.gender = Gender.FEMALE.ToString();
                            mother.name = mother_name;
                            mother.address = address;

                            if (!string.IsNullOrEmpty(phone_house))
                            {
                                mother.phone_home = phone_house;
                            }
                            if (!string.IsNullOrEmpty(mother_phone_cell))
                            {
                                mother.phone_cell = mother_phone_cell;
                            }
                            if (!string.IsNullOrEmpty(mother_phone_office))
                            {
                                mother.user_parents.phone_office = mother_phone_office;
                            }
                            if (!string.IsNullOrEmpty(mother_occupation))
                            {
                                mother.user_parents.occupation = mother_occupation;
                            }
                            repository.AddUser(mother);
                        }
                        
                    }

                    user guardian = null;
                    if (!string.IsNullOrEmpty(guardian_name))
                    {
                        guardian = new user();
                        bool foundDuplicate = false;
                        if (!string.IsNullOrEmpty(guardian_nricpassport))
                        {
                            user found = null;
                            switch (GetIDType(guardian_nricpassport))
                            {
                                case IDTYPE.PASSPORT:
                                    guardian.passportno = guardian_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.passportno == guardian_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        guardian = found;
                                    }
                                    break;
                                case IDTYPE.NEWIC:
                                    guardian.nric_new = guardian_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.nric_new == guardian_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        guardian = found;
                                    }
                                    break;
                                case IDTYPE.BIRTHCERT:
                                    guardian.birthcertno = guardian_nricpassport;
                                    found =
                                        repository.GetUsers().SingleOrDefault(x => x.birthcertno == guardian_nricpassport);
                                    if (found != null)
                                    {
                                        foundDuplicate = true;
                                        guardian = found;
                                    }
                                    break;
                            }
                        }

                        if (!foundDuplicate)
                        {
                            guardian.designation = "";
                            guardian.usergroup = (int)UserGroup.GUARDIAN;
                            guardian.user_parents = new user_parent();
                            guardian.name = guardian_name;
                            guardian.settings = (int)UserSettings.NONE;
                            guardian.address = address;

                            if (!string.IsNullOrEmpty(guardian_phone_home))
                            {
                                guardian.phone_home = guardian_phone_home;
                            }
                            if (!string.IsNullOrEmpty(guardian_phone_hand))
                            {
                                guardian.phone_cell = guardian_phone_hand;
                            }
                            repository.AddUser(guardian);
                        }
                    }
                    
                    if (commit.HasValue && commit.Value)
                    {
                        // save new users
                        repository.Save();

                        // add relationship
                        if (father != null)
                        {
                            var f = new students_guardian();
                            f.parentid = father.id;
                            f.type = (byte) GuardianType.FATHER;
                            student.students_guardians.Add(f);
                        }

                        if (mother != null)
                        {
                            var m = new students_guardian();
                            m.parentid = mother.id;
                            m.type = (byte) GuardianType.MOTHER;
                            student.students_guardians.Add(m);
                        }

                        if (guardian != null)
                        {
                            var g = new students_guardian();
                            g.parentid = guardian.id;
                            g.type = (byte)GuardianType.GUARDIAN;
                            student.students_guardians.Add(g);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return Content(count + ":" + ex.Message);
            }
            
            if (commit.HasValue && commit.Value)
            {
                repository.Save();
                return Content("commited rows " + count);
            }
            return Content("done rows " + count);
        }
#endif
        private IDTYPE GetIDType(string nricpassportstring)
        {
            if (nricpassportstring.Contains("."))
            {
                return IDTYPE.BIRTHCERT;
            }

            if (nricpassportstring.Contains("-"))
            {
                return IDTYPE.NEWIC;
            }

            long nric;
            if (long.TryParse(nricpassportstring, out nric))
            {
                return IDTYPE.NEWIC;
            }

            return IDTYPE.PASSPORT;
        }

        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult Leave()
        {
            return View();
        }

        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult LeaveContent()
        {
            var viewmodel = db.leaves.OrderBy(x => x.name).ToModel();
            return View(viewmodel);
        }

        private DateTime? ParseDate(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            var segments = dateString.Split(new[] {'.'});
            var day = int.Parse(segments[0]);
            var month = int.Parse(segments[1]);
            var year = int.Parse(segments[2]);

            var now = DateTime.Now.Year % 100;
            // if larger than current year than it must be in the last century
            if (year > now)
            {
                year += 1900;
            }
            else
            {
                year += 2000;
            }
            return new DateTime(year,month,day);

        }

        private decimal? GetCellValueAsDecimal(ICell cell)
        {
            decimal? value = null;
            if (cell != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(cell.StringCellValue))
                    {
                        value = decimal.Parse(cell.StringCellValue, NumberStyles.AllowCurrencySymbol |
                                                              NumberStyles.AllowDecimalPoint |
                                                              NumberStyles.AllowThousands);
                    }
                }
                catch
                {
                    // if error then cell is double
                    value = Convert.ToDecimal(cell.NumericCellValue);
                }
            }

            return value;
        }

        private string GetCellValueAsString(ICell cell)
        {
            string value = "";
            if (cell != null)
            {
                try
                {
                    value = cell.StringCellValue;
                }
                catch
                {
                    // if error then cell is numeric
                    value = cell.NumericCellValue.ToString();
                }
            }

            return value;
        }
        
        public ActionResult FormatGender(bool? commit)
        {
            var sb = new StringBuilder();
            foreach (var usr in repository.GetUsers())
            {
                var ic = usr.nric_new;
                if (!string.IsNullOrEmpty(ic))
                {
                    if (ic.Length == 12)
                    {
                        if (usr.gender != Gender.MALE.ToString() && (int.Parse(ic.Substring(11)) % 2) != 0)
                        {
                            usr.gender = Gender.MALE.ToString();
                            sb.AppendFormat("{0}   ", usr.name);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("{0}:{1} ", usr.id , ic);
                    }
                }
            }

            if (commit.HasValue && commit.Value)
            {
                repository.Save();
            }
            
            return Content(sb.ToString());
        }

        public ActionResult AddGuardianInfo(bool? commit)
        {
            var fs = System.IO.File.OpenRead(AppDomain.CurrentDomain.BaseDirectory + "/Content/media/student.xls");
            HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
            var sheet = templateWorkbook.GetSheet("student");
            var count = 1;  // skips header

            while (true)
            {
                var row = sheet.GetRow(count++);
                if (row == null)
                {
                    break;
                }

                var name = GetCellValueAsString(row.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK));

                if (string.IsNullOrEmpty(name))
                {
                    continue;
                }

                //var nricpassport = GetCellValueAsString(row.GetCell(6, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var father_name = GetCellValueAsString(row.GetCell(18, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var father_nricpassport = GetCellValueAsString(row.GetCell(19, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var mother_name = GetCellValueAsString(row.GetCell(23, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var mother_nricpassport = GetCellValueAsString(row.GetCell(24, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var guardian_name = GetCellValueAsString(row.GetCell(31, MissingCellPolicy.RETURN_NULL_AND_BLANK));
                var guardian_nricpassport = GetCellValueAsString(row.GetCell(32, MissingCellPolicy.RETURN_NULL_AND_BLANK));

                var fatherq =
                    repository.GetUsers().Where(
                        x => x.usergroup == (int)UserGroup.GUARDIAN && x.name == father_name);
                var motherq =
                    repository.GetUsers().Where(
                        x => x.usergroup == (int)UserGroup.GUARDIAN && x.name == mother_name);
                var guardianq =
                    repository.GetUsers().Where(
                        x => x.usergroup == (int)UserGroup.GUARDIAN && x.name == guardian_name);

                if (!string.IsNullOrEmpty(father_nricpassport))
                {
                    father_nricpassport = father_nricpassport.Trim().Replace("-", "");
                    fatherq = fatherq.Where(x => x.passportno == father_nricpassport || x.nric_new == father_nricpassport);
                }
                if (!string.IsNullOrEmpty(mother_nricpassport))
                {
                    mother_nricpassport = mother_nricpassport.Trim().Replace("-", "");
                    motherq = motherq.Where(x => x.passportno == mother_nricpassport || x.nric_new == mother_nricpassport);
                }
                if (!string.IsNullOrEmpty(guardian_nricpassport))
                {
                    guardian_nricpassport = guardian_nricpassport.Trim().Replace("-", "");
                    guardianq = guardianq.Where(x => x.passportno == guardian_nricpassport || x.nric_new == guardian_nricpassport);
                }

                foreach (var f in fatherq)
                {
                    f.gender = Gender.MALE.ToString();
                    foreach (var entry in f.students_guardians1)
                    {
                        entry.type = (int)GuardianType.FATHER;
                    }
                }
                foreach (var m in motherq)
                {
                    m.gender = Gender.FEMALE.ToString();
                    foreach (var entry in m.students_guardians1)
                    {
                        entry.type = (int)GuardianType.MOTHER;
                    }
                }
                foreach (var g in guardianq)
                {
                    foreach (var entry in g.students_guardians1)
                    {
                        entry.type = (int)GuardianType.GUARDIAN;
                    }
                }
            }

            if (commit.HasValue && commit.Value)
            {
                repository.Save();
                return Content("committed rows " + count);
            }
            return Content("done rows " + count);
        }

        [HttpGet]
        public ActionResult Teachers()
        {
            var viewmodel =
                new[] { new SelectListItem() { Text = "Select school ...", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult TeachersContent(int school, int year)
        {
            // get subjects
            var subjects = repository.GetSchoolSubjects(school).OrderBy(x => x.name);

            
            var viewmodel = new List<SubjectTeacher>();
            foreach (var entry in subjects)
            {
                var subject = entry;
                var subjectTeacher = db.subject_teachers.Where(x => x.year == year && x.subjectid == subject.id);

                var model = new SubjectTeacher()
                {
                    subjectid = subject.id,
                    subjectname = subject.name
                };

                foreach (var teacher in subjectTeacher.GroupBy(x => x.user))
                {
                    var subjectentry = new SubjectTeacherEntry();
                    subjectentry.teachername = teacher.Key.ToName();
                    foreach (var klass in teacher.OrderBy(x => x.school_class.name))
                    {
                        subjectentry.AllocatedClasses.Add(new IdName(klass.classid, klass.school_class.name));
                    }
                    model.teachers.Add(subjectentry);
                }
                viewmodel.Add(model);
            }

            return View(viewmodel);
        }

        public ActionResult Terms()
        {
            var year = DateTime.Now.Year;

            var viewmodel = new AdminSchoolTermsViewModel();
            viewmodel.yearList = new[] { year }.Union(db.attendance_terms.Select(x => x.year)) 
                .Distinct()
                .OrderByDescending(x => x)
                .Select(x => new SelectListItem() {Text = x.ToString(), Value = x.ToString()});

            foreach (var entry in db.schools)
            {
                var school = entry;
                var schoolTerms = new List<SchoolTerm>();
                var terms = repository.GetSchoolTerms().Where(x => x.schoolid == school.id);
                foreach (var term in terms)
                {
                    var att = term.attendance_terms.SingleOrDefault(x => x.year == year);
                    SchoolTerm row;
                    if (att == null)
                    {
                        row = new SchoolTerm { term = term.name, termid = term.id, year = year };
                    }
                    else
                    {
                        row = att.ToModel();
                    }
                    schoolTerms.Add(row);
                }
                var model = new SchoolTermsViewModel
                {
                    terms = schoolTerms,
                    schoolid = school.id,
                    schoolname = school.name
                };
                viewmodel.terms.Add(model);
            }

            return View(viewmodel);
        }

        [HttpPost]
        [JsonFilter(Param = "schools", RootType = typeof(SchoolTermJSON[]))]
        public ActionResult TermsSave(SchoolTermJSON[] schools)
        {
            foreach (var school in schools)
            {
                foreach (var entry in school.terms)
                {
                    var term = new attendance_term();
                    if (entry.entryid.HasValue)
                    {
                        term = db.attendance_terms.Single(x => x.id == entry.entryid.Value);
                    }
                    else
                    {
                        term.schoolid = school.id;
                        term.year = school.year;
                        db.attendance_terms.InsertOnSubmit(term);
                    }

                    term.termid = entry.termid;
                    term.startdate = new DateTime(school.year, entry.startmonth, entry.startday);
                    term.enddate = new DateTime(school.year, entry.endmonth, entry.endday);
                    term.days = entry.total;
                }
            }

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Attendance Days saved successfully".ToJsonOKMessage());
        }


        public ActionResult populatefeenames()
        {
            foreach (var fee in db.fees)
            {
                if (fee.typeid.HasValue)
                {
                    fee.name = fee.fees_type.name;
                }
            }

            repository.Save();

            return Content("done");
        }
    }
}
