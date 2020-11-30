using System.Collections.Generic;
using System.Web.Mvc;

namespace ioschools.Models.calendar.admin
{
    public class CalendarAdminViewModel : BaseViewModel
    {
        public IEnumerable<CalendarAdminEntry> entries { get; set; }
        public IEnumerable<SelectListItem> yearList { get; set; } 

        public CalendarAdminViewModel(BaseViewModel baseviewmodel)
            : base(baseviewmodel)
        {
        }
    }
}