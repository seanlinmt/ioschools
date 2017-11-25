using System.Collections.Generic;

namespace ioschoolsWebsite.Models.notifications
{
    public class NotificationSendViewModel
    {
        public IEnumerable<IdName> parents { get; set; }
        public string message { get; set; }
        public long studentid { get; set; }
        public string receiver { get; set; }
    }
}