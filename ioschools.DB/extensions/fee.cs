using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;
using ioschools.Data.User;

namespace ioschools.DB
{
    public partial class fee
    {
        public DateTime duedateWithReminders
        {
            get
            {
                return
                    fees_reminders.Select(x => x.paymentdue).Union(new[] {duedate}).OrderByDescending(y => y).First();
            }
        }
    }


    
}
