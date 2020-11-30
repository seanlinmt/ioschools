using System;
using ioschools.Data;
using ioschools.Data.User;

namespace ioschools.Models
{
    public class BaseViewModel
    {
        public bool isLoggedIn { get; set; }
        public string notifications { get; set; }
        public UserAuth userauth { get; set; }
        public string name { get; set; }
        public long? sessionid { get; set; }

        public BaseViewModel()
        {
            userauth = new UserAuth();
        }

        protected BaseViewModel(BaseViewModel viewmodel)
        {
            this.isLoggedIn = viewmodel.isLoggedIn;
            this.userauth = viewmodel.userauth;
            this.name = viewmodel.name;
            this.notifications = viewmodel.notifications;
            this.sessionid = viewmodel.sessionid;
        }

        public string Greeting()
        {
            var time = DateTime.Now;
            string formattedName;
            if (UserSuperGroups.STAFF.HasFlag(userauth.group) && sessionid.HasValue)
            {
                formattedName = string.Format("<strong><a class='underline_hoveroff' href='/users/{0}'>{1}</a></strong>", sessionid.Value, name);
            }
            else
            {
                formattedName = string.Format("<strong>{0}</strong>", name);
            }

            if (time.Hour >= 0 && time.Hour < 12)
            {
                return "Good Morning " + formattedName;
            }

            if (time.Hour >= 12 && time.Hour < 18)
            {
                return "Good Afternoon " + formattedName;
            }

            return "Good Evening " + formattedName;
        }
    }
}