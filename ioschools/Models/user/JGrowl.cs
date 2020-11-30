using System.Collections.Generic;
using ioschools.Data;
using ioschools.Data.User;

namespace ioschools.Models.user
{
    public class JGrowl
    {
        public static readonly JGrowl USER_CHANGEPASSWORD =
            new JGrowl("<a href=\"#\" onclick=\"dialogBox_open('/login/password', 'Change Password');return false;\">Change your password</a>", true);

        public JGrowl()
        {
            
        }

        public JGrowl(string msg, bool stick)
        {
            sticky = stick;
            message = msg;
        }

        public JGrowl(string msg, string parameter, bool stick)
            :this(string.Format(msg, parameter), stick)
        {
        }

        public bool sticky { get; set; }
        public string message { get; set; }
    }

    public static class JGrowlHelper
    {
        public static List<JGrowl> ToNotification(this string info)
        {
            var notifications = new List<JGrowl> {new JGrowl {message = info, sticky = true}};
            return notifications;
        }

        public static List<JGrowl> ToNotification(this ioschools.DB.user info)
        {
            var notifications = new List<JGrowl>();
            if ((info.settings & (int)UserSettings.PASSWORD_RESET) != 0)
            {
                notifications.Add(JGrowl.USER_CHANGEPASSWORD);
            }
            return notifications;
        }
    }
}
