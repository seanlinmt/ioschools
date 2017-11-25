using System;
using System.ComponentModel;

namespace ioschools.Data.User
{
    /// <summary>
    /// needs to be flags so we an use in group action filter
    /// IF YOU WANT TO REPLACE A ROLE, CHECK IN DB THAT IT IS NOT CURRENTLY IN USE
    /// </summary>
    [Flags]
    public enum UserGroup : int
    {
        NONE                        = 0x0000,
        DIRECTOR                    = 0x0001,
        HEAD                        = 0x0002,
        ADMIN                       = 0x0004,
        TEACHER                     = 0x0008,
        FINANCE                     = 0x0020,
        CLERK                       = 0x0040,
        STUDENT                     = 0x0080,
        GUARDIAN                    = 0x0100,
        SUPPORT                     = 0x0200,
        SUPERUSER                   = 0x0400
    }
}
