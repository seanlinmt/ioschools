using System.Collections.Generic;
using System.Linq;
using ioschools.DB;

namespace ioschools.Models.exam.templates
{
    public class ExamTemplate
    {
        public string id { get; set; }
        public string name { get; set; }
        public string creator { get; set; }
        public string description { get; set; }
        public string maxmark { get; set; }
        public string schoolname { get; set; }
        public int schoolid { get; set; }
        public bool isPrivate { get; set; }

        public IEnumerable<ExamSubject> subjects { get; set; }

        public ExamTemplate()
        {
            subjects = Enumerable.Empty<ExamSubject>();
            maxmark = "100"; // default value
            isPrivate = true;
        }
    }

    public static class ExamTemplateHelper
    {
        public static ExamTemplate ToEditModel(this exam_template row)
        {
            return new ExamTemplate()
                       {
                           id = row.id.ToString(),
                           name = row.name,
                           maxmark = row.maxMark.ToString(),
                           schoolid = row.schoolid,
                           description = row.description,
                           subjects = row.exam_template_subjects.OrderBy(x => x.position).ToModel(),
                           isPrivate = row.isprivate
                           
                       };
        }

        public static IEnumerable<ExamTemplate> ToModel(this IEnumerable<exam_template> rows)
        {
            foreach (var row in rows)
            {
                yield return new ExamTemplate()
                                 {
                                     id = row.id.ToString(),
                                     name = row.name,
                                     creator = row.creator.HasValue ? row.user.name : "unknown",
                                     maxmark = row.maxMark.ToString(),
                                     schoolname = row.school.name,
                                     description = row.description,
                                     isPrivate = row.isprivate
                                 };
            }
        }
    }
}