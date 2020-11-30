using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Library;
using ioschools.Library.Helpers;
using ioschools.Models.user;

namespace ioschools.Models.discipline
{
    public class Discipline
    {
        public string id { get; set; }
        public string created { get; set; }
        public string creator_name { get; set; }
        public string reason { get; set; }
        public int? points { get; set; }
        public string type { get; set; }
        public bool canEdit { get; set; }

        // editing
        public IEnumerable<SelectListItem> types { get; set; }
        public List<SelectListItem> ranges { get; set; } 
        public bool isRanged { get; set; }

        public Discipline()
        {
            ranges = new List<SelectListItem>();
        }
    }

    public static class DisciplineHelper
    {
        public static string ToPointModel(this int points)
        {
            if (points < 0)
            {
                return points.ToString();
            }
            return "+" + points;
        }

        public static string ToPointModel(this int? points)
        {
            if (!points.HasValue)
            {
                return "";
            }
            return points.Value.ToPointModel();
        }

        public static Discipline ToModel(this ioschools.DB.students_discipline row, long viewerid)
        {
            return new Discipline()
                                 {
                                     id = row.id.ToString(),
                                     created = row.created.ToString(Constants.DATETIME_SHORT_DATE),
                                     creator_name = row.user1.ToName(false),
                                     points = row.points,
                                     canEdit = row.creator == viewerid,
                                     reason = row.reason,
                                     type = row.type.HasValue? row.conduct.name:""
                                 };
        }

        public static IEnumerable<Discipline> ToModel(this IEnumerable<ioschools.DB.students_discipline> rows, long viewerid)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel(viewerid);
            }
        }
    }
}