using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.eca
{
    public class ECA
    {
        public string name { get; set; }
        public string id { get; set; }
        public int schoolid { get; set; }
        public string school_name { get; set; }
    }

    public static class ECAHelper
    {
        public static ECA ToModel(this ioschools.DB.eca row)
        {
            return new ECA()
                       {
                           id = row.id.ToString(),
                           name = row.name,
                           schoolid = row.schoolid,
                           school_name = row.school.name
                       };
        }

        public static IEnumerable<ECA> ToModel(this IEnumerable<ioschools.DB.eca> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }

}