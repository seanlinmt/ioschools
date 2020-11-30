using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.DB;

namespace ioschools.Models.user.staff
{
    public class EmploymentPeriod
    {
        public string id { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string remarks { get; set; }
    }

    public static class EmploymentPeriodHelper
    {
        public static IEnumerable<EmploymentPeriod> ToModel(this IEnumerable<employment> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }

        public static EmploymentPeriod ToModel(this employment row)
        {
            return new EmploymentPeriod()
            {
                id = row.id.ToString(),
                startDate = row.start_date.HasValue
                                ? row.start_date.Value.ToString(Constants.DATETIME_SHORT_DATE)
                                : "",
                endDate = row.end_date.HasValue
                              ? row.end_date.Value.ToString(Constants.DATETIME_SHORT_DATE)
                              : "",
                remarks = row.remarks
            };
        }
    }
}