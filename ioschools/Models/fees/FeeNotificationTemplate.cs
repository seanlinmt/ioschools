using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;

namespace ioschools.Models.fees
{
    public class FeeNotificationTemplate
    {
        public string id { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    }

    public static class FeeNotificationHelper
    {
        public static FeeNotificationTemplate ToRowModel(this fees_template row)
        {
            return new FeeNotificationTemplate()
            {
                id = row.id.ToString(),
                title = row.title,
                body = row.body
            };
        }

        public static IEnumerable<FeeNotificationTemplate> ToRowModel(this IQueryable<fees_template> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToRowModel();
            }
        }

        public static FeeNotificationTemplate ToModel(this fees_template row)
        {
            return new FeeNotificationTemplate()
                       {
                           body = row.body,
                           id = row.id.ToString(),
                           title = row.title
                       };
        }
    }
}