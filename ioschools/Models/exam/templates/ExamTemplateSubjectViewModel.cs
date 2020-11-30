using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.exam.templates
{
    public class ExamTemplateSubjectViewModel
    {
        public string id { get; set; }
        public string examsubjectname { get; set; }
        public string code { get; set; }
        public IEnumerable<SelectListItem> subjects { get; set; } 
    }
}