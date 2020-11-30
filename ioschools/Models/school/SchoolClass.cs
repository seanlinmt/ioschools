using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Library.Extensions.Models;
using ioschools.DB;

namespace ioschools.Models.school
{
    public class SchoolClass
    {
        public int? id { get; set; }
        public string currentClass { get; set; }
        public string nextClass { get; set; }
        public string schoolYear { get; set; }

        // for editing
        public IEnumerable<SelectListItem> schoolyearList { get; set; }
        public GroupSelectListItem[] nextClassList { get; set; }

        public SchoolClass()
        {
            schoolyearList = Enumerable.Empty<SelectListItem>();
            nextClassList = Enumerable.Empty<GroupSelectListItem>().ToArray();
        }
    }

    public static class SchoolClassHelper
    {
        public static SchoolClass ToModel(this school_class row)
        {
            return new SchoolClass()
            {
                id = row.id,
                currentClass = row.name,
                nextClass = row.nextclass.HasValue?row.school_class1.name:"",
                schoolYear = row.school_year.name
            };
        }

        public static IEnumerable<SchoolClass> ToModel(this IEnumerable<school_class> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }
    }
}