using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.Data.User
{
    [Flags]
    public enum Permission : long
    {
        NONE                        = 0x0,
        SPARE0                      = 0x0000000000000001,
        ATTENDANCE_CREATE           = 0x0000000000000002,
        ATTENDANCE_NOTIFY           = 0x0000000000000004,
        SPARE1                      = 0x0000000000000008,
        SPARE2                      = 0x0000000000000010,

        CONDUCT_ADMIN               = 0x0000000000000020,
        CONDUCT_CREATE              = 0x0000000000000040,
        CONDUCT_NOTIFY              = 0x0000000000000080,
        SPARE25                     = 0x0000000000000100,
        CALENDAR_ADMIN              = 0x0000000000000200,
        SPARE4                      = 0x0000000000000400,


        ECA_ADMIN                   = 0x0000000000000800,
        ECA_CREATE                  = 0x0000000000001000,
        SPARE45                     = 0x0000000000002000,
        SPARE5                      = 0x0000000000004000,
        SPARE6                      = 0x0000000000008000,


        ENROL_ADMIN                 = 0x0000000000010000,
        ENROL_CREATE                = 0x0000000000020000,
        SPARE65                     = 0x0000000000040000,
        SPARE7                      = 0x0000000000080000,
        SPARE8                      = 0x0000000000100000,


        EXAM_ADMIN                  = 0x0000000000200000,
        EXAM_CREATE                 = 0x0000000000400000,
        EXAM_EDIT                   = 0x0000000000800000,
        EXAM_VIEW                   = 0x0000000001000000,
        SPARE86                     = 0x0000000002000000,
        SETTINGS_ADMIN              = 0x0000000004000000,
        SPARE10                     = 0x0000000008000000,


        FEES_ADMIN                  = 0x0000000010000000,
        FEES_UPDATE_STATUS          = 0x0000000020000000,
        SPARE11                     = 0x0000000040000000,
        SPARE12                     = 0x0000000080000000,


        HOMEWORK_CREATE             = 0x0000000100000000,
        SPARE13                     = 0x0000000200000000,
        SPARE14                     = 0x0000000400000000,


        LEAVE_ADMIN                 = 0x0000000800000000,
        LEAVE_REVIEW                = 0x0000001000000000,
        LEAVE_APPLY                 = 0x0000002000000000,
        SPARE145                    = 0x0000004000000000,
        SPARE15                     = 0x0000008000000000,
        STATS_VIEW                  = 0x0000010000000000,


        NEWS_ADMIN                  = 0x0000020000000000,
        NEWS_CREATE                 = 0x0000040000000000,
        NEWS_BROADCAST              = 0x0000080000000000,
        SPARE18                     = 0x0000100000000000,


        TRANSCRIPTS_CREATE          = 0x0000200000000000,
        TRANSCRIPTS_EDIT            = 0x0000400000000000,
        SPARE19                     = 0x0000800000000000,
        SPARE20                     = 0x0001000000000000,


        USERS_VIEW_STUDENTS         = 0x0002000000000000,
        USERS_VIEW_PARENTS          = 0x0004000000000000,
        USERS_VIEW_STAFF            = 0x0008000000000000,
        USERS_EDIT_OWN              = 0x0010000000000000,
        USERS_EDIT_STUDENTS         = 0x0020000000000000,
        USERS_EDIT_PARENTS          = 0x0040000000000000,
        USERS_EDIT_STAFF            = 0x0080000000000000,
        USERS_CREATE                = 0x0100000000000000,
        SPARE22                     = 0x0200000000000000,
        SPARE23                     = 0x0400000000000000,
        WEBSITE_EDIT                = 0x0800000000000000
    }
}
