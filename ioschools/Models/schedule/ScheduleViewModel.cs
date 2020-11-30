using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.schedule
{
    public class ScheduleViewModel : BaseViewModel
    {
        public ScheduleViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }

        public IEnumerable<Schedule> schedules { get; set; }
    }
}