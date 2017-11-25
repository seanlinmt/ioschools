using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschoolsWebsite.Models.eca
{
    public class ECAViewModel
    {
        public IEnumerable<SelectListItem> schools { get; set; } 
        public IEnumerable<ECA> ecas { get; set; }
    }
}