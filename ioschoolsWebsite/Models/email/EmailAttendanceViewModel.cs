using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.email
{
    public class EmailAttendanceViewModel
    {
        public string receiver { get; set; }
        public string offender { get; set; }
        public int days { get; set; }
        public string link { get; set; }
    }
}