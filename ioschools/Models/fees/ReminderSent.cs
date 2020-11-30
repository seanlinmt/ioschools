using System.Collections.Generic;
using System.Linq;
using ioschools.Data;
using ioschools.Models.user;
using ioschools.DB;

namespace ioschools.Models.fees
{
	public class ReminderSent
	{
        public int id { get; set; }
        public string created { get; set; }
        public string due_date { get; set; }
        public string title { get; set; }
        public string sender { get; set; }
        public string receiver { get; set; }
        public bool viewed { get; set; }

	}

    public static class ReminderSentHelper
    {
        public static IEnumerable<ReminderSent> ToModel(this IQueryable<fees_reminder> rows)
        {
            foreach (var row in rows)
            {
                yield return new ReminderSent()
                                 {
                                     id = row.id,
                                     created = row.created.ToShortDateString(),
                                     sender = row.user1.ToName(false),
                                     due_date = row.paymentdue.ToShortDateString(),
                                     title = row.templatename,
                                     receiver = row.user.ToName(false),
                                     viewed = row.viewed
                                 };
            }
        }
    }
}