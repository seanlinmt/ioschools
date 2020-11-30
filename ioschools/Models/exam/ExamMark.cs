using System;
using System.Collections.Generic;
using ioschools.DB;
using ioschools.Models.user;

namespace ioschools.Models.exam
{
    // student and marks
    public class ExamMark
    {
        public static readonly string[] AllowedCharacters = new[]
                                                                {
                                                                    "a", "a*", "a+", "a-", "b", "b+", "c", "c+", "d", "e", "f", "g",
                                                                    "u"
                                                                };
        public IdName student { get; set; }
        public List<IdName> marks { get; set; }

        public ExamMark()
        {
            marks = new List<IdName>();
        }
    }

    public static class ExamMarkHelper
    {
        public static List<ExamMark> ToMarkModel(this IEnumerable<classes_students_allocated> rows)
        {
            var marks = new List<ExamMark>();
            foreach (var row in rows)
            {
                var mark = new ExamMark()
                                 {
                                   student =  new IdName(row.studentid, row.user.ToName())
                                 };
                marks.Add(mark);
            }
            return marks;
        }

        public static bool IsKindyGrade(this string mark)
        {
            if (string.Compare(mark, "vg", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(mark, "g", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(mark, "s", StringComparison.InvariantCultureIgnoreCase) == 0 ||
                string.Compare(mark, "ni", StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                return true;
            }
            return false;
        }
    }
}