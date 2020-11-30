using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.fees
{
    public class LateFeeAlert
    {
        public long parentid { get; set; }
        public string parentname { get; set; }

        public string studentname { get; set; }
        public long studentid { get; set; }
        
        public IEnumerable<SchoolFeeStudent> overdueFees { get; set; }
        public decimal totalUnpaidFees { get; set; }
    }
}