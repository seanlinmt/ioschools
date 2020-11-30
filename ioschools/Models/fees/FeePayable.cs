using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.DB;

namespace ioschools.Models.fees
{
    public class FeePayable
    {
        public string id { get; set; }
        public string name { get; set; }
        public string amount { get; set; }

        public string school_name { get; set; }
        public IEnumerable<SelectListItem> schoolList { get; set; }

    }

    public static class FeePayableHelper
    {
        public static IEnumerable<FeePayable> ToModel(this IQueryable<fees_type> rows)
        {
            foreach (fees_type row in rows)
            {
                yield return row.ToModel();
            }
        }

        public static FeePayable ToModel(this fees_type row)
        {
            return new FeePayable()
                       {
                           id = row.id.ToString(),
                           name = row.name,
                           school_name = row.school.name,
                           amount = row.amount.ToString("n2")
                       };
        }
    }
}