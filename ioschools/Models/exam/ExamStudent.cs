using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Models.school.json;
using ioschools.DB;
using ioschools.Library;

namespace ioschools.Models.exam
{
    public class ExamStudent
    {
        public string date { get; set; }
        public int year { get; set; }
        public string name { get; set; }
        public string marks_total { get; set; }
        public string marks_average { get; set; }
        public string marks_gpa { get; set; } // gpa

        public IEnumerable<GradingRuleJSON> gradingRules { get; set; }

        public List<SubjectMark> marks { get; set; }

        public ExamStudent()
        {
            marks = new List<SubjectMark>();
        }

        public void InitialiseMarks()
        {
            if (marks.Count != 0)
            {
                var total = marks.Where(x => x.mark.HasValue && !x.absent).Sum(x => x.mark.Value);

                // this works because if mark is a grade, mark has no value
                var count = marks.Count(x => x.mark.HasValue && !x.absent);
                if (total != 0)
                {
                    marks_total = total.ToString();
                }
                if (total != 0 && count != 0)
                {
                    marks_average = ((double)total / count).ToString("n2");
                }

                // see if marks can be calculated as gpa
                if (marks.Any(x => x.grade.gradepoint.HasValue))
                {
                    var total_gpa = marks.Where(x => x.grade.gradepoint.HasValue && !x.absent).Sum(x => x.grade.gradepoint.Value);
                    marks_gpa = ((double)total_gpa / count).ToString("n2");
                }
            }
        }
    }

    public static class ExamStudentHelper
    {
        public static IEnumerable<ExamStudent> ToModel(this IEnumerable<exam_mark> rows)
        {
            var result = new List<ExamStudent>();
            foreach (var exam in rows.OrderBy(x => x.exam.exam_date).GroupBy(x => x.exam))
            {
                var es = new ExamStudent();
                es.date = exam.Key.exam_date.ToString(Constants.DATETIME_STANDARD);
                es.year = exam.Key.exam_date.Year;
                es.name = exam.Key.name;
                foreach (var entry in exam.OrderBy(x => x.exam_subjectid))
                {
                    var examMark = entry;

                    // get rules if not already known
                    if (es.gradingRules == null)
                    {
                        if (examMark.exam.exam_classes.First().school_class.school_year.grades_method != null)
                        {
                            es.gradingRules = examMark.exam.exam_classes.First()
                                .school_class
                                .school_year
                                .grades_method
                                .grades_rules
                                .ToModel()
                                .ToArray();
                        }
                        else
                        {
                            es.gradingRules = Enumerable.Empty<GradingRuleJSON>();
                        }
                        
                    }
                    
                    var mark = new SubjectMark();
                    if (!examMark.absent)
                    {
                        mark.AddMark(examMark.mark, es.gradingRules);    
                    }
                    else
                    {
                        mark.AddMark("x", es.gradingRules);
                    }
                    mark.subject_name = examMark.exam_subject.name;
                    mark.code = examMark.exam_subject.code;
                    mark.absent = examMark.absent;
                    
                    try
                    {
                        var count = examMark.exam.exam_marks.Count(x => x.exam_subjectid == examMark.exam_subjectid &&
                                                                        !string.IsNullOrEmpty(x.mark) && 
                                                                        !x.absent);

                        var total = examMark.exam.exam_marks
                            .Where(x => x.exam_subjectid == examMark.exam_subjectid && 
                                !string.IsNullOrEmpty(x.mark) && 
                                !x.absent)
                            .Sum(x => short.Parse(x.mark));
                        if (count == 0)
                        {
                            mark.mark_average = "-";
                        }
                        else
                        {
                            mark.mark_average = ((double)total / count).ToString("n2");    
                        }
                        
                    }
                    catch
                    {
                        mark.mark_average = "-";
                    }

                    es.marks.Add(mark);
                }

                // analyse marks
                es.InitialiseMarks();

                result.Add(es);
            }
            return result;
        }
    }
}