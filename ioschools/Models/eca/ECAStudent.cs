using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data.User;
using ioschools.DB;

namespace ioschools.Models.eca
{
    public class ECAStudent
    {
        public string id { get; set; }
        public int ecaid { get; set; }
        public int schoolid { get; set; }
        public string school_name { get; set; }
        public string name { get; set; }
        public string post { get; set; }
        public string achievement { get; set; }
        public string year { get; set; }
        public string type { get; set; }
        public string remarks { get; set; }
    }

    public static class ECAStudentHelper
    {
        public static ECAStudent ToModel(this eca_student row)
        {
            return new ECAStudent()
                       {
                           post = row.post,
                           achievement = row.achievement,
                           ecaid = row.ecaid,
                           id = row.id.ToString(),
                           name = row.eca.name,
                           year = row.year.ToString(),
                           schoolid = row.eca.schoolid,
                           school_name = row.eca.school.name,
                           type = row.type,
                           remarks = row.remarks
                           
                       };
        }

        public static IEnumerable<ECAStudent> ToModel(this IEnumerable<eca_student> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}