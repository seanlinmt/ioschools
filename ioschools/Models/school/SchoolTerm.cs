using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Library.Helpers;
using ioschools.DB;

namespace ioschools.Models.school
{
    public class SchoolTerm
    {
        public string id { get; set; }
        public string term { get; set; }
        public int termid { get; set; }
        public int year { get; set; }
        public IEnumerable<SelectListItem> startDayList { get; set; }
        public IEnumerable<SelectListItem> startMonthList { get; set; }
        public IEnumerable<SelectListItem> endDayList { get; set; }
        public IEnumerable<SelectListItem> endMonthList { get; set; }
        public int schooldays { get; set; }

        public SchoolTerm()
        {
            startDayList = DateHelper.GetDayList();
            startMonthList = DateHelper.GetMonthList();
            endDayList = DateHelper.GetDayList();
            endMonthList = DateHelper.GetMonthList();
        }
    }

    public static class SchoolTermHelper
    {
        public static SchoolTerm ToModel(this attendance_term row)
        {
            return new SchoolTerm()
                       {
                           id = row.id.ToString(),
                           startDayList = DateHelper.GetDayList(row.startdate.Day),
                           startMonthList = DateHelper.GetMonthList(row.startdate.Month),
                           endDayList = DateHelper.GetDayList(row.enddate.Day),
                           endMonthList = DateHelper.GetMonthList(row.enddate.Month),
                           schooldays = row.days,
                           year = row.year,
                           termid = row.termid,
                           term = row.school_term.name
                       };
        }
    }
}