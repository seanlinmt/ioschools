using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.stats
{
    public class StatsViewModel : BaseViewModel
    {
        public int currentYear { get; set; }
        public IEnumerable<SelectListItem> from_year { get; set; }
        public IEnumerable<SelectListItem> to_year { get; set; }

        public StatsViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }
    }
}