using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.calendar.admin
{
    public class CalendarAdminEntry
    {
        public string id { get; set; }
        public DateTime date { get; set; }
        public bool isHoliday { get; set; }
        public string description { get; set; }
    }

    public static class CalendarAdminEntryHelper
    {
        public static IEnumerable<CalendarAdminEntry> ToAdminModel(this IEnumerable<ioschools.DB.calendar> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToAdminModel();
            }
        }

        public static CalendarAdminEntry ToAdminModel(this ioschools.DB.calendar row)
        {
            return new CalendarAdminEntry()
            {
                id = row.id.ToString(),
                date = row.date,
                description = row.details,
                isHoliday = row.isHoliday
            };
        }
    }

}