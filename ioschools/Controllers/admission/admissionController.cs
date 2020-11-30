using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using clearpixels.Logging;
using ioschools.DB.repository;
using ioschools.Library.email;
using ioschools.Library.Helpers;
using ioschools.Library.Lucene;
using ioschools.Models.email;
using ioschools.Models.enrolment;
using ioschools.Models.user;

namespace ioschools.Controllers.admission
{
    public class admissionController : baseController
    {
        public ActionResult Index(string message)
        {
            var viewmodel = new AdmissionViewModel(baseviewmodel);
            viewmodel.message = message;
            
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
        public ActionResult Save(Gender? child_sex, int enrol_year, int school, int year, string child_name,  
            string child_race, string child_dialect, string child_address,
            int child_dob_day, int child_dob_month, int child_dob_year, string child_pob,
            string child_citizenship, string child_birthcertno, string child_passportnric, bool child_bumi,
            string child_religion, HttpPostedFileBase child_photo, string child_previous_school,
            HttpPostedFileBase child_report, string child_previous_class, string child_leaving_reason, bool? child_handicap,
            bool? child_learning_problems, string child_disability_details,
            // parents fields
            string parent1_designation, string parent1_name, string parent1_passportnric, string parent1_occupation, 
            string parent1_employer, string parent1_race,
            string parent1_dialect, bool? parent1_bumi, string parent1_marital, string parent1_citizenship,
            string parent1_religion, string parent1_officephone, string parent1_homephone, string parent1_handphone,
            string parent1_email, string parent1_address,
            string parent2_designation, string parent2_name, string parent2_passportnric, string parent2_occupation, 
            string parent2_employer, string parent2_race,
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
            string[] sibling_name, string[] sibling_nric,
            // other
            GuardianType? applicant_relationship)
        {
            var admissionHandler = new Admission(repository);
            var result = admissionHandler.Process(child_sex.HasValue?child_sex.Value:Gender.MALE, enrol_year, school, year, child_name,
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
                                                  applicant_relationship, false);

            Syslog.Write(ErrorLevel.INFORMATION, "Online admission submitted: " + result);

            if (result != AdmissionStatus.SUCCESS)
            {
                switch (result)
                {
                    case AdmissionStatus.DUPLICATEEMAIL:
                        return Redirect("/admission#emailexist");
                    case AdmissionStatus.NOEMAIL:
                        return Redirect("/admission#noemail");
                    case AdmissionStatus.NOID:
                        return Redirect("/admission#noid");
                    case AdmissionStatus.INCORRECT_NRIC_PASSPORT:
                        return Redirect("/admission#incorrectnricpassport");
                    case AdmissionStatus.UNKNOWN:
                        return Redirect("/admission#fail");
                    default:
                        return Redirect("/admission#fail");
                }
            }

            // send email with further instructions
            if (admissionHandler.father != null && !string.IsNullOrEmpty(admissionHandler.father.email))
            {
                var email = new EmailRegistrationViewModel();
                email.applicantName = admissionHandler.father.ToName();
                email.email = admissionHandler.father.email;
                email.password = admissionHandler.password_father;
                this.SendEmail(EmailViewType.REGISTRATION, 
                    email,
                    " School Online Enrolment", email.email, admissionHandler.father.ToName());
                LuceneUtil.UpdateLuceneIndex(admissionHandler.father);

            }
            if (admissionHandler.mother != null && !string.IsNullOrEmpty(admissionHandler.mother.email))
            {
                var email = new EmailRegistrationViewModel();
                email.applicantName = admissionHandler.mother.ToName();
                email.email = admissionHandler.mother.email;
                email.password = admissionHandler.password_mother;
                this.SendEmail(EmailViewType.REGISTRATION,
                    email,
                    " School Online Enrolment", email.email, admissionHandler.mother.ToName());
                LuceneUtil.UpdateLuceneIndex(admissionHandler.mother);
            }
            if (admissionHandler.guardian != null && !string.IsNullOrEmpty(admissionHandler.guardian.email))
            {
                var email = new EmailRegistrationViewModel();
                email.applicantName = admissionHandler.guardian.ToName();
                email.email = admissionHandler.guardian.email;
                email.password = admissionHandler.password_guardian;
                this.SendEmail(EmailViewType.REGISTRATION,
                    email,
                    " School Online Enrolment", email.email, admissionHandler.guardian.ToName());
                LuceneUtil.UpdateLuceneIndex(admissionHandler.guardian);
            }

            new Thread(() =>
                           {
                               using (var repo = new Repository())
                               {
                                   var pplToNotify = repo.GetRegistrationNotifications();
                                   foreach (var registrationNotification in pplToNotify)
                                   {
                                       var usr = registrationNotification.user;
                                       if (!string.IsNullOrEmpty(usr.email))
                                       {
                                           EmailHelper.SendEmail(EmailViewType.REGISTRATION_NOTIFICATION,
                                                      null,
                                                      "New  School Online Enrolment",
                                                      usr.email,
                                                      usr.ToName());
                                       }
                                   }
                               }
                           }).Start();

            LuceneUtil.UpdateLuceneIndex(admissionHandler.student);

            return Redirect("/admission#success");
        }
        
        public ActionResult Siblings()
        {
            return View();
        }


    }
}
