using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.exam
{
    public class ReportCardExamSelectViewModel
    {
        public List<SelectListItem> yearList { get; set; }
        public List<SelectListItem> termList { get; set; }
        public IEnumerable<SelectListItem> examList { get; set; }
        public long studentid { get; set; }
        public string student_name { get; set; }
    }
}