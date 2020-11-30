using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.fees.viewmodel
{
    public class ReminderViewModel
    {
        public IEnumerable<SelectListItem> parents { get; set; }
        public IEnumerable<SelectListItem> templates { get; set; } 
        public List<LateFeeAlert> alerts { get; set; }

        public string nextduedate { get; set; }

        public ReminderViewModel()
        {
            alerts = new List<LateFeeAlert>();
        }

    }
}