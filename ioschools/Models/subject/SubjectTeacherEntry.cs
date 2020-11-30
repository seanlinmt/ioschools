using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.subject
{
    public class SubjectTeacherEntry
    {
        public IEnumerable<SelectListItem> teachers { get; set; }
        public string teachername { get; set; }
        public IEnumerable<SelectListItem> classes { get; set; }
        public List<IdName> AllocatedClasses { get; set; }

        public SubjectTeacherEntry()
        {
            AllocatedClasses = new List<IdName>();
        }
    }
}