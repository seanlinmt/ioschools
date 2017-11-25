using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ioschools.Data.User
{
    public enum MaritalStatus
    {
        [Description("Single")]
        SINGLE,
        [Description("Married")]
        MARRIED,
        [Description("Separated")]
        SEPARATED,
        [Description("Divorced")]
        DIVORCED,
        [Description("Widowed")]
        WIDOWED,
    }
}
