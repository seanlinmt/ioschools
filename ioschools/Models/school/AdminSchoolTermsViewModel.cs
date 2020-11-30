using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.school
{
    public class AdminSchoolTermsViewModel
    {
        public IEnumerable<SelectListItem> yearList { get; set; }
        public List<SchoolTermsViewModel> terms { get; set; }

        public AdminSchoolTermsViewModel()
        {
            terms = new List<SchoolTermsViewModel>();
        }
    }
}