using System.Collections.Generic;
using ioschools.DB;

namespace ioschools.Models.exam.templates
{
    public class ExamSubject
    {
        public long id { get; set; }
        public string examsubjectname { get; set; }
        public string code { get; set; }
        public string subjectname { get; set; }
    }

    public static class ExamSubjectHelper
    {
        public static IEnumerable<ExamSubject> ToModel(this IEnumerable<exam_template_subject> rows)
        {
            foreach (var row in rows)
            {
                yield return new ExamSubject()
                                 {
                                     code = row.code,
                                     id = row.id,
                                     examsubjectname = row.name,
                                     subjectname = row.subjectid.HasValue?row.subject.name:""
                                 };
            }
        }
    }
}