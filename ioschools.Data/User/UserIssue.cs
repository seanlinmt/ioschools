using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace ioschools.Data.User
{
    [Flags]
    public enum UserIssue : int
    {
        NONE                    = 0x0000000,
        [Description("Parent/guardian has no child")]
        GUARDIAN_NOCHILD        = 0x00000001,
        [Description("User inactive but has no leaving date")]
        USER_NOLEAVINGDATE      = 0x00000002,
        [Description("User active but has no admission/start date")]
        USER_NOSTARTDATE        = 0x00000004,
        [Description("Teacher has no subject assigned. <a href='/admin'>Assign subject teachers here</a>")]
        TEACHER_NOSUBJECT       = 0x00000008,
        [Description("Staff has no employment info")]
        STAFF_NOEMPLOYMENT      = 0x00000010,
        [Description("Student has not being assigned a class")]
        STUDENT_NOCLASS         = 0x00000020,
        [Description("Student has no enrolment info")]
        STUDENT_NOENROLMENT     = 0x00000040,
        [Description("Student has more than 1 father/mother")]
        STUDENT_MANYPARENTS     = 0x00000080,
        [Description("Student has no birthday")]
        STUDENT_NOBIRTHDAY     =  0x00000100

    }
}
