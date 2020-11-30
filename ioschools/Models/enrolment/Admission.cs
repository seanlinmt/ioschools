using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Data.User;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.DB.repository;
using ioschools.Library;
using ioschools.Library.File;
using ioschools.Library.FileUploader;
using ioschools.Models.user;

namespace ioschools.Models.enrolment
{
    public class Admission
    {
        private IRepository repository { get; set; }
        public ioschools.DB.user student { get; set; }
        public ioschools.DB.user father { get; set; }
        public ioschools.DB.user mother { get; set; }
        public ioschools.DB.user guardian { get; set; }

        // passwords
        public string password_father { get; set; }
        public string password_mother { get; set; }
        public string password_guardian { get; set; }

        public Admission(IRepository repo)
        {
            repository = repo;
            student = null;
            father = null;
            mother = null;
            guardian = null;
        }

        private static IdType GetIDType(string nricpassportstring)
        {
            nricpassportstring = nricpassportstring.Trim();
            if (nricpassportstring.Contains("-") && 
                nricpassportstring.Length == 14)
            {
                return IdType.NEWIC;
            }

            long nric;
            if (long.TryParse(nricpassportstring, out nric))
            {
                if (nricpassportstring.Length == 12)
                {
                    return IdType.NEWIC;
                }
            }

            return IdType.PASSPORT;
        }
        
        public AdmissionStatus Process(Gender child_sex, int enrol_year, int school, int year, string child_name,  
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
            string[] sibling_name, string[] sibling_nric,
            GuardianType? applicant_relationship, bool internalsubmission)
        {
            var noemail = true;   // to ensure at least parent/guardian has email
            var emails = new List<string>();

            // sanitize inputs
            parent1_email = (parent1_email??"").Trim().ToLower();
            parent2_email = (parent2_email??"").Trim().ToLower();
            guardian_email = (guardian_email??"").Trim().ToLower();

            if (!string.IsNullOrEmpty(child_passportnric))
            {
                child_passportnric = child_passportnric.Trim().Replace("-", "");
            }
            if (!string.IsNullOrEmpty(parent1_passportnric))
            {
                parent1_passportnric = parent1_passportnric.Trim().Replace("-", "");
            }
            if (!string.IsNullOrEmpty(parent2_passportnric))
            {
                parent2_passportnric = parent2_passportnric.Trim().Replace("-", "");
            }
            if (!string.IsNullOrEmpty(guardian_passportnric))
            {
                guardian_passportnric = guardian_passportnric.Trim().Replace("-", "");
            }
            

            // check that student does not already exist
            if (!string.IsNullOrEmpty(child_passportnric))
            {
                student = repository.GetUserByNewNRIC(child_passportnric);
                // only match student usergroup to prevent accidently matching with parents or teachers
                if (student != null && student.usergroup != (int)UserGroup.STUDENT)
                {
                    Syslog.Write(ErrorLevel.WARNING, "NRIC incorrect match: " + child_passportnric);
                    return AdmissionStatus.INCORRECT_NRIC_PASSPORT;
                }
            }

            if (student == null && !string.IsNullOrEmpty(child_passportnric))
            {
                student = repository.GetUsers().SingleOrDefault(x => string.Compare(x.passportno, child_passportnric, true) == 0);
                // only match student usergroup to prevent accidently matching with parents or teachers
                if (student != null && student.usergroup != (int)UserGroup.STUDENT)
                {
                    Syslog.Write(ErrorLevel.WARNING, "NRIC incorrect match: " + child_passportnric);
                    return AdmissionStatus.INCORRECT_NRIC_PASSPORT;
                }
            }

            if (student == null)
            {
                // student
                student = new ioschools.DB.user();
                student.usergroup = (int)UserGroup.STUDENT;
                student.settings = (int)UserSettings.INACTIVE;
                student.email = "";
                student.name = child_name;
                student.gender = child_sex.ToString();
                student.race = child_race;
                student.dialect = child_dialect;
                student.dob = new DateTime(child_dob_year, child_dob_month, child_dob_day);
                student.pob = child_pob;
                student.citizenship = child_citizenship;
                student.birthcertno = child_birthcertno;
                student.religion = child_religion;
                student.isbumi = child_bumi;
                student.address = child_address;

                var childidtype = GetIDType(child_passportnric);
                switch (childidtype)
                {
                    case IdType.NEWIC:
                        student.nric_new = child_passportnric;
                        break;
                    case IdType.PASSPORT:
                        student.passportno = child_passportnric;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                repository.AddUser(student);
            }

            // father
            password_father = clearpixels.crypto.Utility.GetRandomString(uppercase: true);
            if (!string.IsNullOrEmpty(parent1_name))
            {
                // see if we can find this parent
                if (!string.IsNullOrEmpty(parent1_passportnric))
                {
                    father = repository.GetUserByNewNRIC(parent1_passportnric);
                }

                if (father == null && !string.IsNullOrEmpty(parent1_passportnric))
                {
                    father = repository.GetUsers().SingleOrDefault(x => string.Compare(x.passportno, parent1_passportnric, true) == 0);
                }

                if (father == null)
                {
                    father = new ioschools.DB.user();
                    father.usergroup = (int)UserGroup.GUARDIAN;
                    father.settings = (int)UserSettings.NONE;
                    father.designation = parent1_designation;
                    father.name = parent1_name;
                    father.gender = Gender.MALE.ToString();
                    father.race = parent1_race;
                    father.dialect = parent1_dialect;
                    father.citizenship = parent1_citizenship;
                    father.address = parent1_address;
                    father.religion = parent1_religion;
                    father.isbumi = parent1_bumi;

                    var fatheridtype = GetIDType(parent1_passportnric);
                    switch (fatheridtype)
                    {
                        case IdType.PASSPORT:
                            father.passportno = parent1_passportnric;
                            break;
                        case IdType.NEWIC:
                            father.nric_new = parent1_passportnric;
                            father.dob = parent1_passportnric.ToDOB();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    
                    if (!string.IsNullOrEmpty(parent1_email))
                    {
                        // checks that email address is not in use
                        if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, parent1_email, true) == 0) != null)
                        {
                            Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + parent1_email);
                            return AdmissionStatus.DUPLICATEEMAIL;
                        }
                        emails.Add(parent1_email);
                        father.email = parent1_email;
                        father.passwordhash = Utility.GeneratePasswordHash(parent1_email, password_father);
                        noemail = false;
                    }
                    father.phone_home = parent1_homephone;
                    father.phone_cell = parent1_handphone;
                    father.user_parents = new user_parent();
                    father.marital_status = parent1_marital;
                    father.user_parents.phone_office = parent1_officephone;
                    father.user_parents.occupation = parent1_occupation;
                    father.user_parents.employer = string.IsNullOrEmpty(parent1_employer)?"":parent1_employer.Trim();
                    repository.AddUser(father);
                }
                else
                {
                    if (string.IsNullOrEmpty(father.email))
                    {
                        // let's see if we can update email information
                        if (!string.IsNullOrEmpty(parent1_email))
                        {
                            // checks that email address is not in use
                            if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, parent1_email, true) == 0) != null)
                            {
                                Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + parent1_email);
                                return AdmissionStatus.DUPLICATEEMAIL;
                            }
                            emails.Add(parent1_email);
                            father.email = parent1_email;
                            father.passwordhash = Utility.GeneratePasswordHash(parent1_email, password_father);
                            noemail = false;
                        }
                    }
                    else
                    {
                        noemail = false;
                    }
                }
            }

            password_mother = clearpixels.crypto.Utility.GetRandomString(uppercase: true);
            if (!string.IsNullOrEmpty(parent2_name))
            {
                // see if we can find this parent
                if (!string.IsNullOrEmpty(parent2_passportnric))
                {
                    mother = repository.GetUserByNewNRIC(parent2_passportnric);
                }

                if (mother == null && !string.IsNullOrEmpty(parent2_passportnric))
                {
                    mother = repository.GetUsers().SingleOrDefault(x => string.Compare(x.passportno, parent2_passportnric, true) == 0);
                }

                if (mother == null)
                {
                    mother = new ioschools.DB.user();
                    mother.usergroup = (int)UserGroup.GUARDIAN;
                    mother.settings = (int)UserSettings.NONE;
                    mother.designation = parent2_designation;
                    mother.name = parent2_name;
                    mother.gender = Gender.FEMALE.ToString();
                    mother.race = parent2_race;
                    mother.dialect = parent2_dialect;
                    mother.citizenship = parent2_citizenship;
                    mother.address = parent2_address;
                    mother.religion = parent2_religion;
                    mother.isbumi = parent2_bumi;

                    var motheridtype = GetIDType(parent2_passportnric);
                    switch (motheridtype)
                    {
                        case IdType.PASSPORT:
                            mother.passportno = parent2_passportnric;
                            break;
                        case IdType.NEWIC:
                            mother.nric_new = parent2_passportnric;
                            mother.dob = parent2_passportnric.ToDOB();
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    
                    if (!string.IsNullOrEmpty(parent2_email))
                    {
                        // checks that email address is not in use
                        if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, parent2_email, true) == 0) != null)
                        {
                            Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + parent2_email);
                            return AdmissionStatus.DUPLICATEEMAIL;
                        }
                        if (!emails.Contains(parent2_email))
                        {
                            emails.Add(parent2_email);
                            mother.email = parent2_email;
                            mother.passwordhash = Utility.GeneratePasswordHash(parent2_email, password_mother);
                            noemail = false;
                        }
                    }
                    mother.phone_home = parent2_homephone;
                    mother.phone_cell = parent2_handphone;
                    mother.user_parents = new user_parent();
                    mother.marital_status = parent2_marital;
                    mother.user_parents.phone_office = parent2_officephone;
                    mother.user_parents.occupation = parent2_occupation;
                    mother.user_parents.employer = string.IsNullOrEmpty(parent2_employer) ? "" : parent2_employer.Trim(); 

                    repository.AddUser(mother);
                }
                else
                {
                    if (string.IsNullOrEmpty(mother.email))
                    {
                        // let's see if we can update email information
                        if (!string.IsNullOrEmpty(parent2_email))
                        {
                            // checks that email address is not in use
                            if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, parent2_email, true) == 0) != null)
                            {
                                Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + parent2_email);
                                return AdmissionStatus.DUPLICATEEMAIL;
                            }
                            emails.Add(parent2_email);
                            mother.email = parent2_email;
                            mother.passwordhash = Utility.GeneratePasswordHash(parent2_email, password_mother);
                            noemail = false;
                        }
                    }
                    else
                    {
                        noemail = false;
                    }
                }
            }

            password_guardian = clearpixels.crypto.Utility.GetRandomString(uppercase:true);
            if (!string.IsNullOrEmpty(guardian_name))
            {
                // see if we can find this parent
                if (!string.IsNullOrEmpty(guardian_passportnric))
                {
                    guardian = repository.GetUserByNewNRIC(guardian_passportnric);
                }

                if (guardian == null && !string.IsNullOrEmpty(guardian_passportnric))
                {
                    guardian = repository.GetUsers().SingleOrDefault(x => string.Compare(x.passportno, guardian_passportnric) == 0);
                }

                if (guardian == null)
                {
                    guardian = new ioschools.DB.user();
                    guardian.usergroup = (int)UserGroup.GUARDIAN;
                    guardian.settings = (int)UserSettings.NONE;
                    guardian.designation = guardian_designation;
                    guardian.name = guardian_name;
                    guardian.gender = guardian_sex.ToString();
                    guardian.race = guardian_race;
                    guardian.dialect = guardian_dialect;
                    guardian.citizenship = guardian_citizenship;
                    guardian.address = guardian_address;
                    guardian.religion = guardian_religion;
                    guardian.isbumi = guardian_bumi;

                    var guardianidtype = GetIDType(guardian_passportnric);
                    switch (guardianidtype)
                    {
                        case IdType.NEWIC:
                            guardian.nric_new = guardian_passportnric;
                            guardian.dob = guardian_passportnric.ToDOB();
                            break;
                        case IdType.PASSPORT:
                            guardian.passportno = guardian_passportnric;
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                    
                    if (!string.IsNullOrEmpty(guardian_email))
                    {
                        // checks that email address is not in use
                        if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, guardian_email) == 0) != null)
                        {
                            Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + guardian_email);
                            return AdmissionStatus.DUPLICATEEMAIL;
                        }
                        if (!emails.Contains(guardian_email))
                        {
                            guardian.email = guardian_email;
                            guardian.passwordhash = Utility.GeneratePasswordHash(guardian_email, password_guardian);
                            noemail = false;
                        }
                    }
                    guardian.phone_home = guardian_homephone;
                    guardian.phone_cell = guardian_handphone;
                    guardian.user_parents = new user_parent();
                    guardian.marital_status = guardian_marital;
                    guardian.user_parents.phone_office = guardian_officephone;
                    guardian.user_parents.occupation = guardian_occupation;
                    guardian.user_parents.employer = string.IsNullOrEmpty(guardian_employer) ? "" : guardian_employer.Trim();

                    repository.AddUser(guardian);
                }
                else
                {
                    if (string.IsNullOrEmpty(guardian.email))
                    {
                        // let's see if we can update email information
                        if (!string.IsNullOrEmpty(guardian_email))
                        {
                            // checks that email address is not in use
                            if (repository.GetUsers().SingleOrDefault(x => string.Compare(x.email, guardian_email, true) == 0) != null)
                            {
                                Syslog.Write(ErrorLevel.WARNING, "Enrolment using existing email " + guardian_email);
                                return AdmissionStatus.DUPLICATEEMAIL;
                            }
                            emails.Add(guardian_email);
                            guardian.email = guardian_email;
                            guardian.passwordhash = Utility.GeneratePasswordHash(guardian_email, password_guardian);
                            noemail = false;
                        }
                    }
                    else
                    {
                        noemail = false;
                    }
                }
            }

            // check that there's an email
            if (noemail && !internalsubmission)
            {
                Syslog.Write(ErrorLevel.WARNING, string.Format("No email specified: 1:{0} 2:{1} 3:{2}", parent1_email, parent2_email, guardian_email));
                return AdmissionStatus.NOEMAIL;
            }

            // check that student is not already enrol for the same year
            // should we? possiblity that student may leave and come back in the same year

            /////////////////////// SAVE ////////////////////
            repository.Save();

            // save photo
            if (child_photo != null)
            {
                var photouploader = new FileHandler(child_photo.FileName, UploaderType.PHOTO, null);
                photouploader.Save(child_photo.InputStream);
                var image = new user_image();
                image.url = photouploader.url;
                student.user_image = image;
            }

            // save child report
            if (child_report != null)
            {
                var reportuploader = new FileHandler(child_report.FileName, UploaderType.REGISTRATION, student.id);
                reportuploader.Save(child_report.InputStream);
                var file = new user_file();
                file.filename = child_report.FileName;
                file.url = reportuploader.url;
                student.user_files.Add(file);
            }

            // siblings
            for (int i = 0; i < sibling_name.Length; i++)
            {
                var name = sibling_name[i];
                var nric = sibling_nric[i];
                if (!string.IsNullOrEmpty(nric))
                {
                    nric = nric.Trim().Replace("-", "");

                    // try to find user
                    var sibling = repository.GetUserByNewNRIC(nric);

                    if ((sibling != null && sibling.usergroup != (int)UserGroup.STUDENT) ||
                        sibling == null)
                    {
                        // try pasport
                        sibling = repository.GetUsers().SingleOrDefault(x => string.Compare(x.passportno, nric, true) == 0);
                    }

                    if (sibling != null)
                    {
                        var s = new sibling();
                        s.otherid = student.id;
                        sibling.siblings1.Add(s);
                    }
                }
            }

            // relationship
            // parent
            if (father != null)
            {
                var f = new students_guardian();
                f.parentid = father.id;
                f.type = (byte) GuardianType.FATHER;
                if (!student.students_guardians.Any(x => x.parentid == f.parentid && x.type == f.type))
                {
                    student.students_guardians.Add(f);
                }
            }

            if (mother != null)
            {
                var m = new students_guardian();
                m.parentid = mother.id;
                m.type = (byte) GuardianType.MOTHER;
                if (!student.students_guardians.Any(x => x.parentid == m.parentid && x.type == m.type))
                {
                    student.students_guardians.Add(m);
                }
            }

            if (guardian != null)
            {
                var g = new students_guardian();
                g.parentid = guardian.id;
                g.type = (byte) GuardianType.GUARDIAN;
                if (!student.students_guardians.Any(x => x.parentid == g.parentid && x.type == g.type))
                {
                    student.students_guardians.Add(g);
                }
            }

            // registration
            var r = new registration();
            r.enrollingYear = enrol_year;
            r.created = DateTime.Now;
            r.schoolid = school;
            r.schoolyearid = year;
            r.studentid = student.id;
            r.status = RegistrationStatus.PENDING.ToString();
            r.schoolid = school;
            r.schoolyearid = year;
            r.previous_school = child_previous_school;
            r.leaving_reason = child_leaving_reason;
            r.disability_details = child_disability_details;
            r.hasLearningProblem = child_learning_problems;
            r.isHandicap = child_handicap;
            r.previous_class = child_previous_class;
            if (applicant_relationship.HasValue)
            {
                switch (applicant_relationship)
                {
                    case GuardianType.FATHER:
                        r.applicantid = father.id;
                        break;
                    case GuardianType.MOTHER:
                        r.applicantid = mother.id;
                        break;
                    case GuardianType.GUARDIAN:
                        r.applicantid = guardian.id;
                        break;
                }
            }

            repository.AddRegistration(r);

            // success
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return AdmissionStatus.UNKNOWN;
            }

            return AdmissionStatus.SUCCESS;
        }
    }
}