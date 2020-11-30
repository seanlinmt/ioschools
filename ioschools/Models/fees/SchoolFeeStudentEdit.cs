using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.fees
{
    public class SchoolFeeStudentEdit : SchoolFeeStudent
    {
        public IEnumerable<SelectListItem> statusList { get; set; } 
    }
}