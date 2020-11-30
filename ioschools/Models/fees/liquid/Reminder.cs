using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotLiquid;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.Models.user;
using ioschools.DB.repository;
using clearpixels.crypto;

namespace ioschools.Models.fees.liquid
{
    public class Reminder : Drop
    {
        public string name_parent { get; set; }
        public string name_director { get; set; }

        public DateTime date_today { get; set; }
        public DateTime date_expired { get; set; } // when fee was due
        public DateTime date_extended { get; set; }  // when is next payment due
        public IEnumerable<string> date_reminders { get; set; } // date of letters
        public string pay_to { get; set; }
        public string pay_total { get; set; }
        public string url_finance { get; set; }
        public List<Children> children { get; set; }
        public string uniqueid { get; set; }

        public Reminder(string uniqueid = null)
        {
            this.uniqueid = uniqueid;
            if (string.IsNullOrEmpty(uniqueid))
            {
                this.uniqueid = Utility.GetRandomString(10);
            }
            children = new List<Children>();
            pay_to = "";
            url_finance = "http://www.ioschools.edu.my/finance/parent?id=" + this.uniqueid;

            date_reminders = Enumerable.Empty<string>();
        }

        public void Initialise(ReminderJSON reminder)
        {
            date_today = DateTime.UtcNow;
            date_extended = reminder.date_due;

            using (var repository = new Repository())
            {
                foreach (var entry in reminder.children)
                {
                    var student = repository.GetUser(entry.studentid);
                    if (student == null)
                    {
                        continue;
                    }
                    var child = new Children();
                    child.name = student.ToName(false);

                    var klass =
                        student.classes_students_allocateds.FirstOrDefault(x => x.year == reminder.date_due.Year);

                    if (klass != null)
                    {
                        child.@class = klass.school_class.name;
                    }

                    children.Add(child);


                    if (string.IsNullOrEmpty(name_parent))
                    {
                        // we haven't populate parent info yet
                        var parent = student.students_guardians.SingleOrDefault(x => x.parentid == reminder.parentid);
                        if (parent != null)
                        {
                            name_parent = parent.user1.ToName();
                        }
                    }

                }

                // calculate total fees and latest overdue fees
                var fees = repository.GetFees().Where(x => reminder.children.Where(y => y.selected).SelectMany(y => y.feeids).ToArray().Contains(x.id));

                pay_total = fees.Sum(x => x.amount).ToString("n2");
                

                // get date of most recent related letter
                if (fees.SelectMany(x => x.fees_reminders).Count() != 0)
                {
                    date_reminders = fees.SelectMany(x => x.fees_reminders).Select(y => y.created).ToArray().Select(z => z.ToString(Constants.DATETIME_STANDARD)).Distinct();
                    date_expired = fees.SelectMany(x => x.fees_reminders).Max(y => y.paymentdue);
                }
                else
                {
                    // no reminders been sent yet so take the largest due date
                    date_expired = fees.Max(x => x.duedate);
                }

                

                // populate director name
                var director = repository.GetUsers(null, null, null, null, UserGroup.DIRECTOR, null, null, null,
                                                      reminder.date_due.Year, null).FirstOrDefault();

                if (director != null)
                {
                    name_director = director.ToName(false);
                }
            }
        }
    }
}