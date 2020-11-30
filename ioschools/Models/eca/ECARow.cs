using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.eca
{
    public class ECARow
    {
        public ECA eca { get; set; }
        public IEnumerable<SelectListItem> schoolList { get; set; }

        public ECARow()
        {
            eca = new ECA();
        }
    }
}