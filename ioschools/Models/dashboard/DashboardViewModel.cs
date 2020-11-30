using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.dashboard
{
    public class DashboardViewModel : BaseViewModel
    {
        public IEnumerable<SelectListItem> yearList { get; set; }

        public DashboardViewModel(BaseViewModel viewmodel) : base(viewmodel)
        {
            
        }
    }
}