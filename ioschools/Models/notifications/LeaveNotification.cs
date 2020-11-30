using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.notifications
{
    public class LeaveNotification
    {
        public long leavetakenID { get; set; }

        // approval
        public string receiver { get; set; }
        public string applicant { get; set; }
        public long applicantid { get; set; }

        // status update
        public string status { get; set; }
        public string reason { get; set; }
    }
}