using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.Data.User
{
    public static class UserSuperGroups
    {
        public const UserGroup STAFF =
            UserGroup.DIRECTOR | UserGroup.HEAD | UserGroup.ADMIN | UserGroup.TEACHER | UserGroup.FINANCE |
            UserGroup.CLERK | UserGroup.SUPPORT;

        public const UserGroup SUPERSTAFF = UserGroup.DIRECTOR | UserGroup.HEAD | UserGroup.ADMIN;
    }
}
