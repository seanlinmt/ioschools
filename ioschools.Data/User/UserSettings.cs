using System;

namespace ioschools.Data.User
{
    [Flags]
    public enum UserSettings
    {
        NONE            = 0,
        PASSWORD_RESET  = 0x00000001,
        INACTIVE        = 0x00000002,
        PENDING         = 0X00000004,
        LEAVING         = 0X00000008
    }
}