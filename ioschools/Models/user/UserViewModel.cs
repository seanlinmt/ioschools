using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.user
{
    public class UserViewModel : BaseViewModel
    {
        public User usr { get; set; }

        public UserViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }
    }
}