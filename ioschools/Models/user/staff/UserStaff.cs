using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;

namespace ioschools.Models.user.staff
{
    public class UserStaff
    {
        public string salaryGrade { get; set; }
        public string incomeTax { get; set; }
        public string epf { get; set; }
        public string socso { get; set; }
        public string spouseName { get; set; }
        public string spouseEmployer { get; set; }
        public string spouseEmployerAddress { get; set; }
        public string spousePhoneCell { get; set; }
        public string spousePhoneWork { get; set; }
        public IEnumerable<EmploymentPeriod> employmentPeriods { get; set; }

        public UserStaff()
        {
            employmentPeriods = Enumerable.Empty<EmploymentPeriod>();
        }
    }

    public static class UserStaffHelper
    {
        public static UserStaff ToModel(this user_staff row)
        {
            if (row == null)
            {
                return new UserStaff();
            }

            return new UserStaff()
                       {
                           salaryGrade = row.salary_grade,
                           incomeTax = row.income_tax,
                           epf = row.epf,
                           socso = row.socso,
                           spouseName = row.spouse_name,
                           spouseEmployer = row.spouse_employer,
                           spouseEmployerAddress = row.spouse_employer_address,
                           spousePhoneCell = row.spouse_phone_cell,
                           spousePhoneWork = row.spouse_phone_work,
                           employmentPeriods =
                               row.user.employments != null
                                   ? row.user.employments.OrderByDescending(x => x.start_date).ToModel()
                                   : Enumerable.Empty<EmploymentPeriod>()
                       };
        }
    }
}