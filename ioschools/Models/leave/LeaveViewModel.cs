using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.leave
{
    public class LeaveViewModel : BaseViewModel
    {
        public string staffname { get; set; }
        public long staffid { get; set; }
        public IEnumerable<StaffLeave> leaves { get; set; }
        public IEnumerable<LeaveTaken> takenleaves { get; set; }
        public IEnumerable<SelectListItem> yearList { get; set; } 

        // permissions
        public bool canApplyLeave { get; set; }

        public LeaveViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            leaves = Enumerable.Empty<StaffLeave>();
            takenleaves = Enumerable.Empty<LeaveTaken>();
        }

    }
}