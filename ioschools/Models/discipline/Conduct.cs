using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;

namespace ioschools.Models.discipline
{
    public class Conduct
    {
        public int? id { get; set; }
        public string name { get; set; }
        public int? min { get; set; }
        public int? max { get; set; }
        public bool isdemerit { get; set; }
    }

    public static class ConductHelper
    {
        public static Conduct ToModel(this conduct row)
        {
            return new Conduct()
                       {
                           id = row.id,
                           name = row.name,
                           min = row.min,
                           max = row.max,
                           isdemerit = row.isdemerit
                       };
        }

        public static IEnumerable<Conduct> ToModel(this IEnumerable<conduct> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    
    }

}