using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public classes_teachers_allocated GetClassPeriod(int year, int day, int schoolid, int classid, TimeSpan periodStart, TimeSpan periodEnd)
        {
            // allow new start time to be = existing end time or new end time = existing start time
            //               exist.start           exist.end
            //  new.start    new.end
            //                                     new.start     new.end
            //           new.start                        new.end 
            return db.classes_teachers_allocateds.Where(x =>
                                                        x.year == year &&
                                                        x.day == day &&
                                                        x.classid == classid &&
                                                        x.schoolid == schoolid &&
                                                        ((x.time_start == periodStart && x.time_end == periodEnd) ||
                                                         (periodStart < x.time_end && periodStart >= x.time_start) ||
                                                         (periodEnd > x.time_start && periodEnd <= x.time_end) ||
                                                         (periodStart <= x.time_start && periodEnd >= x.time_end)
                                                        )).FirstOrDefault();
        }
    }
}
