using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.fees
{
    public class ReminderChildrenJSON
    {
        public long studentid { get; set; }
        public bool selected { get; set; }
        public IEnumerable<long> feeids { get; set; } 
    }
}