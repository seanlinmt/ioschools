using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschoolsWebsite.Models.school;
using ioschools.DB;
using ioschoolsWebsite.Models.user;

namespace ioschoolsWebsite.Models.admin
{
    public class AdminViewModel : BaseViewModel
    {
        // school days
        public int year { get; set; }


        // users
        public IEnumerable<UserBase> enrolmentNotifiers { get; set; }

        // statistics
        public bool cacheTimer1Min { get; set; }
        public bool cacheTimer5Min { get; set; }
        public bool cacheTimer10Min { get; set; }
        public bool cacheTimer60Min { get; set; }
        public long mailQueueLength { get; set; }

        // change log
        public IEnumerable<SelectListItem> changeLogYears { get; set; }

        public AdminViewModel(BaseViewModel viewdata) : base(viewdata)
        {
            enrolmentNotifiers = Enumerable.Empty<UserBase>();
            

            year = DateTime.Now.Year;
        }
    }
}