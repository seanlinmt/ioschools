using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.fees
{
    public class ReminderJSON
    {
        public long parentid { get; set; }
        public int templateid { get; set; }

        public bool useEmail { get; set; }
        public bool useSMS { get; set; }

        public DateTime date_due { get; set; }

        public IEnumerable<ReminderChildrenJSON> children { get; set; } 
    }
}