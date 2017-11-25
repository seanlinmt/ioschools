using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ioschools.Data.Leave
{
    public enum LeaveDaySegment : byte
    {
        [Description("start")]
        START = 1,
        [Description("mid")]
        MIDDLE = 2,
        [Description("end")]
        END = 3
    }
}
