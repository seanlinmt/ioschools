using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.Lucene;
using ioschools.Models.email;
using ioschools.Models.enrolment;
using ioschools.Models.user;
using NPOI.HSSF.UserModel;

namespace ioschools.Controllers.admission
{
    public class enrolmentController : baseController
    {
        [PermissionFilter(perm = Permission.ENROL_CREATE)]
        public ActionResult Add()
        {
            var viewmodel = new AdmissionViewModel(baseviewmodel);
            viewmodel.schools = new[] { new SelectListItem() { Text = "Select school ...", Value = "" } }.Union(
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
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult AddNotifier(long id)
        {
            var usr = repository.GetUser(id);
            usr.registration_notifications.Add(new registration_notification());

            repository.Save();

            return RedirectToAction("SelectableBlock", "Users", new {id = id});
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult DeleteNotifier(long id)
        {
            try
            {
                repository.DeleteRegistrationNotificationByUser(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("User removed successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Delete(long id)
        {
            var enrol = repository.GetRegistration(id);
            if (enrol == null)
            {
                return Json("Unable to find entry".ToJsonFail());
            }

            try
            {
                repository.DeleteRegistraion(enrol);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Registration deleted successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Edit(long id)
        {
            var enrol = repository.GetRegistration(id);
            if (enrol == null)
            {
                return Json("Unable to find entry".ToJsonFail());
            }

            var viewmodel = new EnrolmentViewModel(baseviewmodel);
            viewmodel.enrolment = enrol.ToModel();

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult EditStudent(long id)
        {
            var enrol = repository.GetRegistration(id);
            if (enrol == null)
            {
                return Json("Unable to find entry".ToJsonFail());
            }

            var viewmodel = enrol.ToModel();

            var view = this.RenderViewToString("EditStudent", viewmodel);

            return Json(view.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult ModifyStudent()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Export(int? school, int? yearList, RegistrationStatus? status, int? classyear)
        {
            var results = repository.GetRegistration(school, status, yearList, classyear);
            var currentYear = DateTime.Now.Year;
            var ms = new MemoryStream();
            using (var fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/EnrolmentTemplate.xls",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var templateWorkbook = new HSSFWorkbook(fs, true);
                var rowcount = 2; // skips header
                    
                foreach (var entry in results.OrderBy(x => x.user.name))
                {
                    var sheet = templateWorkbook.GetSheetAt(0);

                    var row = sheet.CreateRow(rowcount);
                    var col = 0;
                    row.CreateCell(col++).SetCellValue(rowcount - 1);
                    if (entry.enrollingYear.HasValue)
                    {
                        row.CreateCell(col++).SetCellValue(entry.enrollingYear.Value);
                    }
                    else
                    {
                        col++;
                    } 
                    
                    if (entry.user.dob.HasValue)
                    {
                        row.CreateCell(col++).SetCellValue(entry.user.dob.Value.ToShortDateString());
                    }
                    else
                    {
                        col++;
                    }
                    row.CreateCell(col++).SetCellValue(entry.user.gender.Substring(0,1));
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(entry.user.ToName()));
                    row.CreateCell(col++).SetCellValue(entry.status);
                    row.CreateCell(col++).SetCellValue(entry.school.name);
                    row.CreateCell(col++).SetCellValue(entry.school_year.name);
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(entry.previous_school));
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(entry.previous_class));
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(entry.leaving_reason));

                    // disability
                    var disabilityString = new StringBuilder();
                    if (entry.isHandicap.HasValue && entry.isHandicap.Value)
                    {
                        disabilityString.Append("HANDICAP ");
                    }
                    if (entry.hasLearningProblem.HasValue && entry.hasLearningProblem.Value)
                    {
                        disabilityString.Append("LEARNING_PROBLEM ");
                    }
                    disabilityString.Append(entry.disability_details);
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(disabilityString.ToString()));

                    // address of student (applicant)
                    row.CreateCell(col++).SetCellValue(SecurityElement.Escape(entry.user.address));

                    // siblings
                    var siblingsList = new List<string>();
                    if (entry.user.students_guardians.Count == 0)
                    {
                        row.CreateCell(col++).SetCellValue("No Guardians");
                    }
                    else if (entry.user.students_guardians.First().user1.students_guardians1.Count == 0)
                    {
                        row.CreateCell(col++).SetCellValue("No siblings");
                    }
                    else
                    {
                        var siblings = entry.user.students_guardians.First().user1.students_guardians1.Select(x => x.user);
                        foreach (var sibling in siblings)
                        {
                            var sclass =
                                sibling.classes_students_allocateds.Where(x => x.year == currentYear).SingleOrDefault();
                            if (sclass != null)
                            {
                                siblingsList.Add(string.Format("{0} ({1}) ", sibling.ToName(), sclass.school_class.name));
                            }
                        }
                        row.CreateCell(col++).SetCellValue(string.Join(", ", siblingsList.ToArray()));
                    }

                    var father = entry.user.ToParent(GuardianType.FATHER);
                    if (father != null)
                    {
                        row.CreateCell(col++).SetCellValue(father.ToName());
                        row.CreateCell(col++).SetCellValue(father.ToContactString());
                    }
                    else
                    {
                        col++;
                        col++;
                    }
                    var mother = entry.user.ToParent(GuardianType.MOTHER);
                    if (mother != null)
                    {
                        row.CreateCell(col++).SetCellValue(mother.ToName());
                        row.CreateCell(col++).SetCellValue(mother.ToContactString());
                    }
                    else
                    {
                        col++;
                        col++;
                    }
                    var guardian = entry.user.ToParent(GuardianType.GUARDIAN);
                    if (guardian != null)
                    {
                        row.CreateCell(col++).SetCellValue(guardian.ToName());
                        row.CreateCell(col++).SetCellValue(guardian.ToContactString());
                    }
                    else
                    {
                        col++;
                        col++;
                    }
                    
                    rowcount++;
                    
                }
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("Enrolment_{0}.xls", DateTime.Now.ToShortDateString().Replace("/", "")));

        }

        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Index()
        {
            var viewmodel = new EnrolmentListViewModel(baseviewmodel);

            // init year list
            viewmodel.yearlist.Add(new SelectListItem(){Text = "All",Value = ""});

            var years =
                repository.GetRegistration(null, null, null, null).Where(x => x.enrollingYear.HasValue).Select(
                    x => x.enrollingYear.Value).Distinct().OrderBy(x => x);
            foreach (var year in years)
            {
                viewmodel.yearlist.Add(new SelectListItem(){Text = year.ToString(), Value = year.ToString()});
            }

            // init schools
            viewmodel.schools =
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

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult List(int? school, RegistrationStatus? status, int? year, int rows, int page, int? classyear,
            string term)
        {
            IEnumerable<registration> results = repository.GetRegistration(school, status, year, classyear);

            if (!string.IsNullOrEmpty(term))
            {
                long userid;
                if (long.TryParse(term, out userid))
                {
                    results = repository.GetUsers().Where(x => x.id == userid).SelectMany(x => x.registrations);
                }
                else
                {
                    var search = new LuceneSearch();
                    var ids = search.UserSearch(term.ToLowerInvariant());
                    results = results.Where(x => ids.Select(y => y.docId).Contains(x.user.id)).AsEnumerable();
                    results = results.Join(ids, x => x.user.id, y => y.docId, (x, y) => new { x, y.score })
                        .OrderByDescending(x => x.score).Select(x => x.x);
                }
            }
            else
            {
                results = results.OrderByDescending(x => x.created);
            }

            var enrols = results.Skip(rows * (page - 1)).Take(rows).ToEnrolJqGrid();
            var records = results.Count();
            var total = (records / rows);
            if (records % rows != 0)
            {
                total++;
            }

            enrols.page = page;
            enrols.records = records;
            enrols.total = total;

            return Json(enrols);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Update(RegistrationStatus enrol_status, string message, long id,
            int? enrol_year, int? enrol_school, int? enrol_schoolyear, 
            int? admission_day, int? admission_month, int? admission_year, int? left_day, int? left_month, int? left_year,
            string previous_school, string previous_class, string leaving_reason, bool? handicap, bool? learning_problems,
            string disability_details, bool? sendmessage
            )
        {
            if (!enrol_schoolyear.HasValue)
            {
                return Json("Enrolling school year is not specified".ToJsonFail());
            }

            if (!enrol_school.HasValue)
            {
                return Json("Enrolling school is not specified".ToJsonFail());
            }

            var r = repository.GetRegistration(id);
            r.status = enrol_status.ToString();
            r.reviewer = sessionid;
            r.reviewMessage = message;
            r.previous_school = previous_school;
            r.previous_class = previous_class;
            r.leaving_reason = leaving_reason;
            r.isHandicap = handicap;
            r.hasLearningProblem = learning_problems;
            r.disability_details = disability_details;
            try
            {
                if (admission_year.HasValue)
                {
                    try
                    {
                        r.admissionDate = new DateTime(admission_year.Value, admission_month.Value, admission_day.Value);
                    }
                    catch (Exception ex)
                    {
                        return Json("Invalid admission date".ToJsonFail());
                    }
                    
                }
                else
                {
                    r.user.settings = r.user.SetFlag(UserSettings.INACTIVE);
                    r.admissionDate = null;
                }

                if (left_year.HasValue)
                {
                    try
                    {
                        r.leftDate = new DateTime(left_year.Value, left_month.Value, left_day.Value);
                    }
                    catch (Exception ex)
                    {

                        return Json("Invalid leaving date".ToJsonFail());
                    }
                    
                }
                else
                {
                    r.user.settings = r.user.UnsetFlag(UserSettings.INACTIVE);
                    r.leftDate = null;
                }

                r.schoolid = enrol_school.Value; // checked
                r.enrollingYear = enrol_year;
                r.schoolyearid = enrol_schoolyear.Value; // checked
            
                repository.Save();

                // send email to applicant
                if (sendmessage.HasValue && 
                    sendmessage.Value &&
                    r.user1 != null &&
                    !string.IsNullOrEmpty(r.user1.email))
                {
                    var email = new EmailRegistrationUpdateViewModel();
                    email.applicantName = r.user1.ToName();
                    email.message = message.ToHtmlBreak();

                    this.SendEmail(
                            EmailViewType.REGISTRATION_UPDATE,
                            email,
                            " School Enrolment Update",
                            r.user1.email,
                            email.applicantName);
                }
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Enrolment updated successfully".ToJsonOKMessage());
        }
        
        [HttpPost]
        [PermissionFilter(perm = Permission.ENROL_CREATE)]
        public ActionResult Save(Gender child_sex, int enrol_year, int school, int year, string child_name,
            string child_race, string child_dialect, string child_address,
            int child_dob_day, int child_dob_month, int child_dob_year, string child_pob,
            string child_citizenship, string child_birthcertno, string child_passportnric, bool child_bumi,
            string child_religion, HttpPostedFileBase child_photo, string child_previous_school,
            HttpPostedFileBase child_report, string child_previous_class, string child_leaving_reason, bool? child_handicap,
            bool? child_learning_problems, string child_disability_details,
            // parents fields
            string parent1_designation, string parent1_name, string parent1_passportnric,
            string parent1_occupation, string parent1_employer, string parent1_race,
            string parent1_dialect, bool? parent1_bumi, string parent1_marital, string parent1_citizenship,
            string parent1_religion, string parent1_officephone, string parent1_homephone, string parent1_handphone,
            string parent1_email, string parent1_address,
            string parent2_designation, string parent2_name, string parent2_passportnric,
            string parent2_occupation, string parent2_employer, string parent2_race,
            string parent2_dialect, bool? parent2_bumi, string parent2_marital, string parent2_citizenship,
            string parent2_religion, string parent2_officephone, string parent2_homephone, string parent2_handphone,
            string parent2_email, string parent2_address,
            // guardian fields
            string guardian_designation, string guardian_name, Gender? guardian_sex,
            string guardian_passportnric, string guardian_occupation, string guardian_employer, string guardian_race,
            string guardian_dialect, bool? guardian_bumi, string guardian_marital, string guardian_citizenship,
            string guardian_religion, string guardian_officephone, string guardian_homephone, string guardian_handphone,
            string guardian_email, string guardian_address,
            // other siblings
            string[] sibling_name, string[] sibling_nric)
        {
            var admissionHandler = new Admission(repository);

            var result = admissionHandler.Process(child_sex, enrol_year, school, year, child_name,
                                                  child_race, child_dialect, child_address,
                                                  child_dob_day, child_dob_month, child_dob_year, child_pob,
                                                  child_citizenship, child_birthcertno, child_passportnric, child_bumi,
                                                  child_religion, child_photo, child_previous_school,
                                                  child_report, child_previous_class, child_leaving_reason,
                                                  child_handicap,
                                                  child_learning_problems, child_disability_details,
                                                  parent1_designation, parent1_name, parent1_passportnric,
                                                  parent1_occupation, parent1_employer, parent1_race,
                                                  parent1_dialect, parent1_bumi, parent1_marital, parent1_citizenship,
                                                  parent1_religion, parent1_officephone, parent1_homephone,
                                                  parent1_handphone,
                                                  parent1_email, parent1_address,
                                                  parent2_designation, parent2_name, parent2_passportnric,
                                                  parent2_occupation, parent2_employer, parent2_race,
                                                  parent2_dialect, parent2_bumi, parent2_marital, parent2_citizenship,
                                                  parent2_religion, parent2_officephone, parent2_homephone,
                                                  parent2_handphone,
                                                  parent2_email, parent2_address,
                                                  guardian_designation, guardian_name, guardian_sex,
                                                  guardian_passportnric, guardian_occupation, guardian_employer,
                                                  guardian_race,
                                                  guardian_dialect, guardian_bumi, guardian_marital,
                                                  guardian_citizenship,
                                                  guardian_religion, guardian_officephone, guardian_homephone,
                                                  guardian_handphone,
                                                  guardian_email, guardian_address, sibling_name, sibling_nric,
                                                  null, true);

            if (result != AdmissionStatus.SUCCESS)
            {
                switch (result)
                {
                    case AdmissionStatus.DUPLICATEEMAIL:
                        ViewData["message"] =
                            "The email address you have specified is currently in use. Please specify a different email address.";
                        break;
                    case AdmissionStatus.NOEMAIL:
                        ViewData["message"] =
                            "No email was specified. The email of either the parents or guardian is required to complete your online registration.";
                        break;
                    case AdmissionStatus.NOID:
                        ViewData["message"] = "You must specify a NRIC or Passport Number.";
                        break;
                    case AdmissionStatus.INCORRECT_NRIC_PASSPORT:
                        ViewData["message"] = "Invalid NRIC or Passport Number.";
                        break;
                    default:
                        ViewData["message"] =
                            "An error has occurred while processing your registration. We are currently looking into the issue.";
                        break;
                }
                return View("Error");
            }
            
            LuceneUtil.UpdateLuceneIndex(admissionHandler.student);

            return Redirect("/enrolment");
        }

        /// <summary>
        /// not to be called directly
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult ShowStudent(long id)
        {
            var r = repository.GetRegistration(id);
            if (r == null)
            {
                return new EmptyResult();
            }

            var canview = r.user.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return new EmptyResult();
            }

            var viewmodel = r.ToModel();

            return View(new[]{viewmodel});
        }

        [PermissionFilter(perm = Permission.ENROL_ADMIN)]
        public ActionResult Single(long id)
        {
            var registration = repository.GetRegistration(id);
            if (registration == null)
            {
                return new EmptyResult();
            }

            return RedirectToAction("single", "users", new {id = registration.studentid});
        }
    }
}
