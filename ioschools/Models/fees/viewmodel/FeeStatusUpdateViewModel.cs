using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.fees.viewmodel
{
    public class FeeStatusUpdateViewModel
    {
        public string duedate { get; set; }

        public int feetypeid { get; set; }
        public string feename { get; set; }
        public int year { get; set; }
        public string schoolname { get; set; }

        public IEnumerable<SchoolFeeUpdateRow> studentList { get; set; }
    }
}