using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschoolsWebsite.Models.user;

namespace ioschoolsWebsite.Models.attendance
{
    public class AttendanceAddViewModel : Attendance
    {
        public IEnumerable<SelectListItem> ecaList { get; set; }

        public AttendanceAddViewModel()
        {
            ecaList = Enumerable.Empty<SelectListItem>();
        }
    }
}