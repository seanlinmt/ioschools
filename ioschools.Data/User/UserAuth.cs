namespace ioschools.Data.User
{
    public class UserAuth
    {
        public Permission perms { get; set; }
        public UserGroup group { get; set; }

        public UserAuth()
        {
            perms = Permission.NONE;
            group = UserGroup.NONE;
        }

        public UserAuth(Permission p, UserGroup g)
        {
            perms = p;
            group = g;
        }
    }
}