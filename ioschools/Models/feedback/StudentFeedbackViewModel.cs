using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.feedback
{
    public class StudentFeedbackViewModel
    {
        public long studentid { get; set; }
        public string studentname { get; set; }
        public List<SelectListItem> staffList { get; set; }

        public StudentFeedbackViewModel()
        {
            staffList = new List<SelectListItem>();
        }
    }
}