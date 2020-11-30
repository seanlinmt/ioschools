using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Models.fees;

namespace ioschools.Models.finance
{
    public class StatementChildren
    {
        public string childname { get; set; }
        public IEnumerable<SchoolFeeStudent> fees { get; set; } 
    }
}