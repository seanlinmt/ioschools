using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschoolsWebsite.Library.Extensions.Models
{
    public class GroupSelectListItem : SelectListItem
    {
        public string Group { get; set; }
    }
}