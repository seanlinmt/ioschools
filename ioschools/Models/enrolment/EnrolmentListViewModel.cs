using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.enrolment
{
    public class EnrolmentListViewModel : BaseViewModel
    {
        public List<SelectListItem> yearlist { get; set; }
        public IEnumerable<SelectListItem> schools { get; set; } 

        public EnrolmentListViewModel(BaseViewModel baseViewModel) :base(baseViewModel)
        {
            yearlist = new List<SelectListItem>();
        }
    }
}