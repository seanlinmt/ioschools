using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.exam.viewmodels
{
    public class ExamDashboardViewModel : BaseViewModel
    {
        public IEnumerable<SelectListItem> schools { get; set; } 
        public IEnumerable<SelectListItem> yearlist { get; set; } 
        public ExamDashboardViewModel(BaseViewModel baseviewmodel) :base(baseviewmodel)
        {
            
        }
    }
}