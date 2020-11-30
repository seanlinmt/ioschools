using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.DB;

namespace ioschools.Models.fees
{
    public class SchoolFeeStudent
    {
        public string id { get; set; }
        public string status { get; set; }
        public string name { get; set; }
        public DateTime? duedate { get; set; }
        public decimal amount { get; set; }
        public int warningsent { get; set; }
        public bool canEdit { get; set; }
    }

    public static class SchoolFeeStudentHelper
    {
        public static IEnumerable<SchoolFeeStudent> ToModel(this IEnumerable<fee> rows, Permission? permission = null)
        {
            foreach (var row in rows.OrderBy(y => y.duedateWithReminders))
            {
                yield return row.ToModel(permission);
            }
        }

        public static string ToSmartDate(this DateTime? row)
        {
            if (!row.HasValue)
            {
                return "";
            }
            if (row.Value < DateTime.Now)
            {
                return string.Format("<span class='font_red bold'>{0}</span>", row.Value.ToString(Constants.DATETIME_SHORT_DATE));
            }

            return row.Value.ToString(Constants.DATETIME_SHORT_DATE);
        }

        public static SchoolFeeStudent ToModel(this fee row, Permission? permission)
        {
            return new SchoolFeeStudent()
                       {
                           amount = row.amount,
                           canEdit = permission.HasValue && ((Permission.FEES_UPDATE_STATUS | Permission.FEES_ADMIN) & permission.Value) != 0,
                           id = row.id.ToString(),
                           name = row.name,
                           duedate = row.status != FeePaymentStatus.PAID.ToDescriptionString()? row.duedateWithReminders : (DateTime?)null,
                           status = row.status.ToEnum<FeePaymentStatus>().ToDescriptionString(),
                           warningsent = row.fees_reminders.Count()
                       };
        }
    }
}