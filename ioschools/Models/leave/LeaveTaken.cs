using System;
using System.Collections.Generic;
using ioschools.Data;
using ioschools.Data.Leave;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.DB;

namespace ioschools.Models.leave
{
    public class LeaveTaken
    {
        public string id { get; set; }
        public string name { get; set; }
        public string start_time { get; set; }
        public string start_date { get; set; }
        public string end_time { get; set; }
        public string end_date { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public bool showReview { get; set; }
        public decimal days { get; set; }
        public bool showDelete { get; set; }
        public string reason { get; set; }
    }

    public static class LeaveTakenHelper
    {
        public static IEnumerable<LeaveTaken> ToModel(this IEnumerable<leaves_taken> rows, bool canapprove, long viewerid, Permission viewer_permissions)
        {
            foreach (var row in rows)
            {
                yield return new LeaveTaken()
                                 {
                                     id = row.id.ToString(),
                                     name = row.leaves_allocated.leave.name,
                                     description = row.details,
                                     end_date = row.enddate.ToString(Constants.DATEFORMAT_DATEPICKER),
                                     end_time = row.endtime.ToEnum<LeaveDaySegment>().ToDescriptionString(),
                                     start_date = row.startdate.ToString(Constants.DATEFORMAT_DATEPICKER),
                                     start_time = row.starttime.ToEnum<LeaveDaySegment>().ToDescriptionString(),
                                     status =  row.ToStatusString(),
                                     showReview = canapprove && row.status == (byte)LeaveStatus.PENDING,
                                     days = row.days,
                                     showDelete = (row.staffid == viewerid || viewer_permissions.HasFlag(Permission.LEAVE_ADMIN)) && row.status != (byte)LeaveStatus.CANCELLED,
                                     reason = row.reason
                                 };
            }
        }

        private static string ToStatusString(this leaves_taken row)
        {
            var status = row.status.ToEnum<LeaveStatus>();
            switch (status)
            {
                case LeaveStatus.PENDING:
                    return string.Format("<span class='tag_orange'>PENDING</span>");
                case LeaveStatus.APPROVED:
                    return string.Format("<span class='tag_green'>APPROVED</span>");
                case LeaveStatus.REJECTED:
                    return string.Format("<span class='tag_red'>REJECTED</span>");
                case LeaveStatus.CANCELLED:
                    return string.Format("<span class='tag_grey'>CANCELLED</span>");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}