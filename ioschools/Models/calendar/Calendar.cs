using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using ioschools.Library.Helpers;
using ioschools.Models.blog;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using ioschools.DB.repository;

namespace ioschools.Models.calendar
{
    public class Calendar
    {
        public static readonly string[] months = new[] { "January", "February", "March", "April", "May", "June", "July", "August ", "September", "October", "November", "December" };
        private const string CALENDAR_KEY = "calendar_key";
        private const string CALENDAR_FILE = "/Content/media/calendar.xls";

        public IEnumerable<CalendarEntry> GetCalendarFromDatabase()
        {
            // try to get from cache first
            var entries = new List<CalendarEntry>();

            using (var repository = new Repository())
            {
                var calendar_entries = repository.GetCalendarEntries();

                foreach (var centry in calendar_entries)
                {
                    var exist =
                        entries.Where(x => x.day == centry.date.Day && x.month == centry.date.Month && x.year == centry.date.Year)
                            .SingleOrDefault();
                    if (exist == null)
                    {
                        // new entry
                        var day = new CalendarEntry()
                        {
                            day = centry.date.Day,
                            month = centry.date.Month,
                            year = centry.date.Year,
                            holiday = centry.isHoliday
                        };
                        day.entry.Add(centry.details);
                        entries.Add(day);
                    }
                    else
                    {
                        exist.entry.Add(centry.details);
                    }
                }
            }

            return entries;
        }

        public IEnumerable<CalendarEntry> GetCalendarFromExcel()
        {
            // try to get from cache first
            var data = HttpRuntime.Cache.Get(CALENDAR_KEY);
            if (data == null)
            {
                var entries = new List<CalendarEntry>();

                using (var fs =
                    new FileStream(
                        AppDomain.CurrentDomain.BaseDirectory + CALENDAR_FILE,
                        FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var templateWorkbook = new HSSFWorkbook(fs, true);
                    var sheet = templateWorkbook.GetSheet("2011");
                    // format: year | month | date | day | entry | holiday
                    int count = 0;
                    int currentYear = 0;
                    int currentDate = 0;
                    int currentMonth = 0;
                    while (true)
                    {
                        var row = sheet.GetRow(count++);
                        if (row == null)
                        {
                            break;
                        }
                        var year = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt();
                        if (year.HasValue)
                        {
                            currentYear = year.Value;
                        }

                        var month = row.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsString();
                        if (!string.IsNullOrEmpty(month))
                        {
                            for (int i = 0; i < months.Length; i++)
                            {
                                if (months[i].IndexOf(month, StringComparison.CurrentCultureIgnoreCase) != -1)
                                {
                                    currentMonth = i + 1;
                                    break;
                                }
                            }
                        }
                        var date = row.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt();
                        if (date.HasValue)
                        {
                            currentDate = date.Value;
                        }
                        var entry = row.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsString();
                        var holiday = row.GetCell(5, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsBoolean();

                        var exist = entries.Where(x => x.day == currentDate && x.month == currentMonth && x.year == currentYear).SingleOrDefault();
                        if (exist == null)
                        {
                            // new entry
                            var day = new CalendarEntry()
                            {
                                day = currentDate,
                                month = currentMonth,
                                year = currentYear,
                                holiday = holiday
                            };
                            day.entry.Add(entry);
                            entries.Add(day);
                        }
                        else
                        {
                            exist.entry.Add(entry);
                        }
                    }
                }
                HttpRuntime.Cache.Insert(CALENDAR_KEY, entries, new CacheDependency(AppDomain.CurrentDomain.BaseDirectory + CALENDAR_FILE));
                data = entries;
            }

            return (List<CalendarEntry>)data;
        }

        public IEnumerable<BlogSummary> GetFutureEvents(int entryCount)
        {
            var now = DateTime.UtcNow;
            var entries = GetCalendarFromDatabase()
                .Where(x => new DateTime(x.year, x.month, x.day) >= now)
                .Take(entryCount).ToModel();
            return entries;
        }

    }
}