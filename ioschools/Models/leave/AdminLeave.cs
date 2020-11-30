using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.leave
{
    public class AdminLeave
    {
        public string id { get; set; }
        public string name { get; set; }
        public int? annualTotal { get; set; }
    }

    public static class AdminLeaveHelper
    {
        public static AdminLeave ToModel(this ioschools.DB.leave row)
        {
            return new AdminLeave()
                       {
                           id = row.id.ToString(),
                           name = row.name,
                           annualTotal = row.defaultTotal
                       };
        }

        public static IEnumerable<AdminLeave> ToModel(this IEnumerable<ioschools.DB.leave> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}