using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ioschools.Library.Helpers
{
    public static class DateHelper
    {
        public static IEnumerable<SelectListItem> GetDayList(int? selectedDay = null, bool emptyFirstOption = false)
        {
            var dayList = new List<SelectListItem>();
            if (emptyFirstOption)
            {
                dayList.Add(new SelectListItem() {Text = "", Value = ""});
            }
            for (int i = 1; i <= 31; i++)
            {
                var val = i.ToString();
                var day = new SelectListItem()
                              {
                                  Text = val,
                                  Value = val,
                                  Selected = selectedDay.HasValue && selectedDay.Value == i
                              };
                dayList.Add(day);
            }
            return dayList;
        }

        public static IEnumerable<SelectListItem> GetMonthList(int? selectedMonth = null, bool emptyFirstOption = false)
        {
            var monthList = new List<SelectListItem>();
            if (emptyFirstOption)
            {
                monthList.Add(new SelectListItem() {Text = "", Value = ""});
            }
            foreach (Month entry in Enum.GetValues(typeof (Month)))
            {
                var month = new SelectListItem()
                                {
                                    Text = entry.ToString(),
                                    Value = entry.ToInt().ToString(),
                                    Selected = selectedMonth.HasValue && selectedMonth.Value == entry.ToInt()
                                };
                monthList.Add(month);
            }
            return monthList;
        }

        public static string ToString(this DateTime? date, string format)
        {
            if (!date.HasValue)
            {
                return "";
            }

            return date.Value.ToString(format);
        }
    }
}