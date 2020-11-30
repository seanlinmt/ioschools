using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.leave
{
    public class LeaveTakenEdit
    {
        public string id { get; set; }
        public IEnumerable<SelectListItem> typeList  { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string description { get; set; }
    }
}