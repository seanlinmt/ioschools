using System.Collections.Generic;
using ioschools.DB;

namespace ioschools.Areas.exams.Models
{
    public class StudentRemark
    {
        public string id { get; set; }
        public string term_name { get; set; }
        public short term_id { get; set; }
        public int year { get; set; }
        public string remark { get; set; }
        public string conduct { get; set; }
    }

    public static class StudentRemarkHelper
    {
        public static StudentRemark ToModel(this students_remark row)
        {
            return new StudentRemark()
                             {
                                 id = row.id.ToString(),
                                 remark = row.remarks,
                                 term_name = row.school_term.name,
                                 term_id = row.term,
                                 year = row.year,
                                 conduct = row.conduct
                             };
        }

       public static IEnumerable<StudentRemark> ToModel(this IEnumerable<students_remark> rows)
       {
           foreach (var row in rows)
           {
               yield return row.ToModel();
           }
       }
    }
}