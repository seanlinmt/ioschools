using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.user.student
{
    public class GuardianViewModel : BaseViewModel
    {
        public IEnumerable<User> students { get; set; }

        public GuardianViewModel(BaseViewModel viewmodel)
            : base(viewmodel)
        {
            students = Enumerable.Empty<User>();
        }
    }
}