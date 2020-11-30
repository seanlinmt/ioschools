using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.eca
{
    public class ECAStudentEditViewModel
    {
        public ECAStudent eca { get; set; }
        public IEnumerable<SelectListItem> schools { get; set; }
        public IEnumerable<SelectListItem> ecaList { get; set; }
        public IEnumerable<SelectListItem> typeList { get; set; }

        public ECAStudentEditViewModel()
        {
            eca = new ECAStudent();
            schools = Enumerable.Empty<SelectListItem>();
            ecaList = Enumerable.Empty<SelectListItem>();
            typeList = Enumerable.Empty<SelectListItem>();
        }
    }
}