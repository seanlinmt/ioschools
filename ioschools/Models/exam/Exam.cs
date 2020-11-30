using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Library;
using ioschools.Models.user;
using clearpixels.Models.jqgrid;

namespace ioschools.Models.exam
{
    public class Exam
    {
        public long id { get; set; }
        public string name { get; set; }
        public string date { get; set; }
        public IEnumerable<IdName> subjects { get; set; }
        public short maxMark { get; set; }

        public List<ExamSection> sections { get; set; }

        public Exam()
        {
            sections = new List<ExamSection>();
        }
    }

    public static class ExamHelper
    {
        public static JqgridTable ToExamsJqGrid(this IEnumerable<ioschools.DB.exam> rows)
        {
            var grid = new JqgridTable();
            foreach (var row in rows)
            {
                var entry = new JqgridRow();
                entry.id = row.id.ToString();
                entry.cell = new object[]
                                 {
                                     row.id,
                                     row.id,
                                     row.ToJqGridNameColumn(),
                                     row.ToDetailsJqgridColumn(),
                                     string.Format("<span class='jqdelete'>delete</span>")
                                 };
                grid.rows.Add(entry);
            }
            return grid;
        }

        private static string ToDetailsJqgridColumn(this ioschools.DB.exam row)
        {
            return string.Format("<div class='bold'>{0}</div><div class='smaller'>{1} / {2} <span class='font_grey'> subjects restricted</span></div>",
                string.Join(", ", row.exam_classes.Select(x => x.school_class.name).ToArray()),
                row.exam_subjects.Count(x => x.subjectid.HasValue),
                row.exam_subjects.Count());
        }

        public static string ToJqGridNameColumn(this ioschools.DB.exam row)
        {
            return string.Format("<h4><a href='/exams/{0}'>{1}</a></h4><div class='font_grey'>{2} - {3}</div><div>{4}</div>",
                row.id,
                row.name,
                row.exam_date.ToString(Constants.DATETIME_SHORT_DATE),
                row.user.ToName(),
                row.description);
        }

        public static Exam ToMarksModel(this ioschools.DB.exam row)
        {
            var exam = new Exam();
            exam.maxMark = row.maxMark;
            exam.id = row.id;
            exam.name = row.name;
            exam.date = row.exam_date.ToString(Constants.DATETIME_SHORT_DATE);
            exam.subjects = row.exam_subjects.OrderBy(x => x.name).Select(x => new IdName(x.id, x.name)).ToArray();

            // gets student entries
            // sections = classes
            var sections = row.exam_classes.ToModel(row.year);

            foreach (var examSection in sections)
            {
                var section = new ExamSection();
                section.class_name = examSection.class_name;
                section.class_id = examSection.class_id;
                foreach (var examMark in examSection.marks)
                {
                    var mark = new ExamMark();
                    mark.student = examMark.student;
                    foreach (var entry in exam.subjects)
                    {
                        var subject = entry;
                        var exist = row.exam_marks
                            .SingleOrDefault(x => x.studentid.ToString() == mark.student.id &&
                                                                        x.exam_subjectid.ToString() == subject.id);
                        var newmark = new IdName();
                        if (exist != null)
                        {
                            // show x if student absent
                            if (exist.absent)
                            {
                                newmark = new IdName(exist.id, "x");
                            }
                            else
                            {
                                newmark = new IdName(exist.id, exist.mark);
                            }

                        }
                        mark.marks.Add(newmark);

                    }
                    section.marks.Add(mark);
                }
                section.marks = section.marks.OrderBy(x => x.student.name).ToList();
                exam.sections.Add(section);
            }

            return exam;
        }
    }
}