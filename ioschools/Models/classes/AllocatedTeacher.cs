using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;
using ioschools.Library;

namespace ioschools.Models.classes
{
    public class AllocatedTeacher
    {
        public long id { get; set; }
        public DayOfWeek day { get; set; }
        public int year { get; set; }
        public string school { get; set; }
        public string subjectname { get; set; }
        public string classname { get; set; }
        public string time_start { get; set; }
        public string time_end { get; set; }
    }

    public static class AllocatedTeacherHelper
    {
        public static IEnumerable<AllocatedTeacher> ToModel(this IEnumerable<classes_teachers_allocated> rows)
        {
            foreach (var row in rows)
            {
                yield return new AllocatedTeacher()
                                 {
                                     id = row.id,
                                     day = (DayOfWeek)row.day,
                                     year = row.year,
                                     school = row.school.name,
                                     subjectname = row.subjectid.HasValue?row.subject.name:"",
                                     classname = row.school_class.name,
                                     time_start = string.Format("{0}:{1}", row.time_start.Hours, row.time_start.Minutes.ToString("d2")),
                                     time_end = string.Format("{0}:{1}", row.time_end.Hours, row.time_end.Minutes.ToString("d2"))
                                 };
            }
        }
    }
}