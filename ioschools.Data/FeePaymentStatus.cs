using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ioschools.Data
{
    public enum FeePaymentStatus
    {
        [Description("")]
        UNKNOWN = 0,
        [Description("UNPAID")]
        UNPAID = 1,
        [Description("PAID")]
        PAID = 2
    }
}
