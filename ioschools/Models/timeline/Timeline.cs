using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using ioschools.Library;
using ioschools.Library.Helpers;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ioschools.Models.timeline
{
    public class Timeline
    {
        private const string TIMELINE_KEY = "historical_timeline_key";
        private const string TIMELINE_FILE = "/Content/media/history.xls";

        public List<Event> GetEvents()
        {
            // try to get from cache first
            var data = HttpRuntime.Cache.Get(TIMELINE_KEY);
            if (data == null)
            {
                var entries = new List<Event>();

                using (FileStream fs =
                    new FileStream(
                        AppDomain.CurrentDomain.BaseDirectory + TIMELINE_FILE,
                        FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
                    var sheet = templateWorkbook.GetSheet("timeline");
                    int count = 0;
                    while (true)
                    {
                        var row = sheet.GetRow(count++);
                        if (row == null)
                        {
                            break;
                        }
                        if (row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt() == null)
                        {
                            break;
                        }
                        var day_start = row.GetCell(2, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt();
                        var day_end = row.GetCell(3, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt();
                        var year = row.GetCell(0, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsInt().Value;
                        var monthstring = row.GetCell(1, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsString();
                        var title = row.GetCell(4, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsString();
                        var description = row.GetCell(5, MissingCellPolicy.RETURN_NULL_AND_BLANK).AsString();
                        var month = string.IsNullOrEmpty(monthstring) ? 1 : (int)Enum.Parse(typeof (Month), monthstring, true);
                        var entry = new Event();
                        entry.start = new DateTime(year, month, day_start??1).ToString("d MMM yyyy");
                        if (day_end.HasValue)
                        {
                            entry.end = new DateTime(year, month, day_end.Value).ToString("d MMM yyyy");
                        }
                        
                        entry.title = title;
                        entry.description = description;
                        entries.Add(entry);
                    }
                    

                }

                HttpRuntime.Cache.Insert(TIMELINE_KEY, entries,
                                         new CacheDependency(AppDomain.CurrentDomain.BaseDirectory + TIMELINE_FILE));
                data = entries;
            }
            return (List<Event>) data;
        }
    }
}