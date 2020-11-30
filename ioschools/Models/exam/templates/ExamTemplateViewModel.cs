using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.exam.templates
{
    public class ExamTemplateViewModel : BaseViewModel
    {
        public ExamTemplate template { get; set; }
        public IEnumerable<SelectListItem> schoolList { get; set; }
        public ExamTemplateViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            template = new ExamTemplate();
        }
    }
}