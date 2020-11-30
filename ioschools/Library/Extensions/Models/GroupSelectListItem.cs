using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Library.Extensions.Models
{
    public class GroupSelectListItem : SelectListItem
    {
        public new string Group { get; set; }
    }
}