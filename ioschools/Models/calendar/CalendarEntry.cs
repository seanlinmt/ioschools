using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.calendar
{
    public class CalendarEntry
    {
        public int year { get; set; }
        public int day { get; set; }
        public int month { get; set; }
        public List<string> entry { get; set; }
        public bool holiday { get; set; }

        public CalendarEntry()
        {
            entry = new List<string>();
        }
    }
}