using System.Collections.Generic;
using ioschools.DB;

namespace ioschools.Models.school.json
{
    public class GradingRuleJSON
    {
        public int? id { get; set; }
        public string grade { get; set; }
        public decimal? gradepoint { get; set; }
        public short? mark { get; set; }
    }

    public static class GradingRuleHelper
    {
        public static GradingRuleJSON ToModel(this grades_rule row)
        {
            return new GradingRuleJSON()
            {
                id = row.id,
                gradepoint = row.gradepoint,
                mark = row.mark,
                grade = row.grade
            };
        }

        public static IEnumerable<GradingRuleJSON> ToModel(this IEnumerable<grades_rule> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}