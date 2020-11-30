using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.feedback
{
    public class FeedbackEmailViewModel
    {
        public string receiver { get; set; }
        public IdName sender { get; set; }
        public IdName student { get; set; }
        public string message { get; set; }
    }
}