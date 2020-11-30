using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.subject.viewmodels
{
    public class AdminSubject
    {
        public string id { get; set; }
        public string subjectname { get; set; }
        public string schoolname { get; set; }
        public int schoolid { get; set; }
    }

    public static class AdminSubjectHelper
    {
        public static IEnumerable<AdminSubject> ToModel(this IEnumerable<ioschools.DB.subject> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }

        public static AdminSubject ToModel(this ioschools.DB.subject row)
        {
            return new AdminSubject()
                       {
                           id = row.id.ToString(),
                           subjectname = row.name,
                           schoolname = row.school.name,
                           schoolid = row.schoolid
                       };
        }
    }
}