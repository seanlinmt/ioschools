using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IQueryable<calendar> GetCalendarEntries(int? year = null)
        {
            if (year.HasValue)
            {
                return db.calendars.Where(x => x.date.Year == year);
            }

            return db.calendars;
        }

        public IQueryable<changelog> GetChangeLogs()
        {
            return db.changelogs.AsQueryable();
        }

        public void AddChangeLog(changelog change)
        {
            db.changelogs.InsertOnSubmit(change);
        }

    }
}
