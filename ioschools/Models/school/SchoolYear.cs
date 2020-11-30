using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.DB;

namespace ioschools.Models.school
{
    public class SchoolYear
    {
        public int? id { get; set; }
        public string name { get; set; }
        public string gradingmethodID { get; set; }
        public string gradingmethodName { get; set; }
        public IEnumerable<SelectListItem> gradingmethodList { get; set; }

        public SchoolYear()
        {
            gradingmethodList = Enumerable.Empty<SelectListItem>();
        }
    }

    public static class SchoolYearHelper
    {
        public static SchoolYear ToModel(this school_year row)
        {
            return new SchoolYear()
                       {
                           id = row.id,
                           name = row.name,
                           gradingmethodID = row.grademethodid.ToString(),
                           gradingmethodName = row.grademethodid.HasValue ? row.grades_method.name : ""
                       };
        }

        public static IEnumerable<SchoolYear> ToModel(this IEnumerable<school_year> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}