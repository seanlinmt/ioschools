using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;
using ioschools.Library;

namespace ioschools.Models.schedule
{
    public class Schedule
    {
        public string start_time { get; set; }
        public string end_time { get; set; }
        public string subject_name { get; set; }
        public string class_name { get; set; }
    }

    public static class ScheduleViewModelHelper
    {
        public static IEnumerable<Schedule> ToSchedule(this IEnumerable<classes_teachers_allocated> rows)
        {
            foreach (var row in rows)
            {
                yield return new Schedule
                                 {
                                     class_name = row.school_class.name,
                                     subject_name = row.subjectid.HasValue?row.subject.name: "",
                                     start_time = string.Format("{0}:{1}", row.time_start.Hours, row.time_start.Minutes.ToString("d2")),
                                     end_time = string.Format("{0}:{1}", row.time_end.Hours, row.time_end.Minutes.ToString("d2"))
                                 };
            }
        }
    }
}