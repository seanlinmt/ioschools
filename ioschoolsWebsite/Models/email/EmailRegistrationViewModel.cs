using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.email
{
    public class EmailRegistrationViewModel
    {
        public string email { get; set; }
        public string password { get; set; }
        public string applicantName { get; set; }
    }
}