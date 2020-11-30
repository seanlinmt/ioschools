using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data.User;
using ioschools.DB;

namespace ioschools.Models.exam
{
    public class ExamSection
    {
        public string class_name { get; set; }
        public int class_id { get; set; }
        public List<ExamMark> marks { get; set; }
        
        public ExamSection()
        {
            marks = new List<ExamMark>();
        }
    }

    public static class ExamSectionHelper
    {
        public static IEnumerable<ExamSection> ToModel(this IEnumerable<exam_class> rows, int year)
        {
            foreach (var row in rows)
            {
                yield return new ExamSection()
                                 {
                                     class_name = row.school_class.name,
                                     class_id = row.classid,
                                     marks = row.school_class.classes_students_allocateds
                                                    .Where(x => x.year == year)
                                                    .ToMarkModel()
                                 };
            }
        }
    }
}