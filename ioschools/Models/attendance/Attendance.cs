using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Library;
using ioschools.Library.Helpers;
using ioschools.Models.user;

namespace ioschools.Models.attendance
{
    public class Attendance
    {
        public long id { get; set; }
        public long studentid { get; set; }
        public string class_name { get; set; }
        public string student_name { get; set; }
        public string date { get; set; }
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public string status { get; set; }
        public string reason { get; set; }
    }

    public static class AttendanceHelper
    {
        public static int GetAttendanceCount(this ioschools.DB.user usr, DateTime startDate, DateTime endDate, AttendanceStatus status, bool forClass)
        {
            var results = usr.attendances.Where(x => x.status == status.ToString() && x.date >= startDate && x.date <= endDate);
            if (forClass)
            {
                results = results.Where(x => x.classid.HasValue);
            }
            else
            {
                results = results.Where(x => x.ecaid.HasValue);
            }

            return results.Count();
        }

        public static int GetAttendanceCount(this ioschools.DB.user usr, int year, AttendanceStatus status, bool forClass)
        {
            var results = usr.attendances.Where(x => x.status == status.ToString() && x.date >= new DateTime(year,1,1) && x.date <= new DateTime(year,12,31));
            if (forClass)
            {
                results = results.Where(x => x.classid.HasValue);
            }
            else
            {
                results = results.Where(x => x.ecaid.HasValue);
            }

            return results.Count();
        }

        public static IEnumerable<Attendance> ToModel(this IEnumerable<ioschools.DB.attendance> rows)
        {
            foreach (var row in rows)
            {
                yield return new Attendance()
                                 {
                                     id = row.id,
                                     class_name = row.classid.HasValue?row.school_class.name: row.eca.name,
                                     student_name = row.user.ToName(),
                                     studentid = row.studentid,
                                     date = row.date.ToString(Constants.DATETIME_SHORT_DATE),
                                     status = row.status.ToEnum<AttendanceStatus>().ToStatusString(),
                                     reason = row.reason
                                 };
            }
        }

        private static string ToStatusString(this AttendanceStatus status)
        {
            switch (status)
            {
                case AttendanceStatus.LATE:
                    return "<span class='status_orange'>LATE</span>";
                case AttendanceStatus.ABSENT:
                    return "<span class='status_red'>ABSENT</span>";
                case AttendanceStatus.EXCUSED:
                    return "<span class='status_blue'>EXCUSED</span>";
                default:
                    return "ERROR";
            }
        }

        public static int[] ToNumberThisWeek(this IEnumerable<ioschools.DB.attendance> rows, DateTime date)
        {
            var dayspan = 0 - ((int)date.DayOfWeek + 1);
            var totalrows = rows.Where(x => x.date >= date.AddDays(dayspan) && x.date <= date).ToArray();
            var late = totalrows.Count(x => x.status == AttendanceStatus.LATE.ToString());
            var absent = totalrows.Count(x => x.status == AttendanceStatus.ABSENT.ToString());

            return new[]{late,absent};
        }

        public static int ToTotalThisWeek(this IEnumerable<ioschools.DB.attendance> rows, DateTime date)
        {
            var dayspan = 0 - ((int)date.DayOfWeek + 1);
            return rows.Count(x => x.date >= date.AddDays(dayspan) && x.date <= date);
        }
    }
}