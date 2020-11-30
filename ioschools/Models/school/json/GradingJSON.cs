using System.Collections.Generic;
using ioschools.DB;

namespace ioschools.Models.school.json
{
    public class GradingJSON
    {
        public int? id { get; set; }
        public string name { get; set; }
        public GradingRuleJSON[] rules { get; set; }  // only used for deserializing
    }

    public static class GradingHelper
    {
        public static GradingJSON ToModel(this grades_method row)
        {
            return new GradingJSON()
            {
                id = row.id,
                name = row.name
            };
        }

        public static IEnumerable<GradingJSON> ToModel(this IEnumerable<grades_method> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}