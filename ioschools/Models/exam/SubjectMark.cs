using System;
using System.Collections.Generic;
using System.Linq;
using ioschools.Data;
using ioschools.Models.school.json;
using ioschools.DB;

namespace ioschools.Models.exam
{
    public class SubjectMark
    {
        public string code { get; set; }
        public string subject_name { get; set; }
        public short? mark { get; private set; }
        public bool absent { get; set; }
        public string mark_average { get; set; }
        public Grade grade { get; set; }

        public void AddMark(string val, IEnumerable<GradingRuleJSON> rules)
        {
            short result;
            if (short.TryParse(val, out result))
            {
                // this is a mark so get grade
                mark = result;
                grade = CalculateGradeAndGP(result, rules);
            }
            else
            {
                if (val == null)
                {
                    val = "";
                }
                grade = new Grade(val);
            }
            
        }

        private Grade CalculateGradeAndGP(short mark, IEnumerable<GradingRuleJSON> rules)
        {
            foreach (var rule in rules.OrderByDescending(x => x.mark))
            {
                if (mark >= rule.mark)
                {
                    return new Grade(rule.grade, rule.gradepoint);
                }
            }
            return new Grade("");
        }
    }

    public static class SubjectMarkHelper
    {
        public static Grade ToGrade(this short mark, school_year schoolyear)
        {
            if (schoolyear.grades_method == null)
            {
                return new Grade("");
            }

            foreach (var rule in schoolyear.grades_method.grades_rules.OrderByDescending(x => x.mark))
            {
                if (mark >= rule.mark)
                {
                    return new Grade(rule.grade, rule.gradepoint);
                }
            }
            return new Grade("");
        }
    }
}