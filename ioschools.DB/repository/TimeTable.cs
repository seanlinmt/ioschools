using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IEnumerable<classes_teachers_allocated> GetStudentTimeTable(long userid, DayOfWeek day, int year)
        {
            var sallocated = db.classes_students_allocateds.SingleOrDefault(x => x.studentid == userid && x.year == year);
            if (sallocated == null)
            {
                return Enumerable.Empty<classes_teachers_allocated>();
            }
            return
                db.classes_teachers_allocateds.Where(x => x.classid == sallocated.classid && 
                                                          x.day == (int)day && x.year == year)
                                                          .OrderBy(x => x.time_start);
        }

        public IEnumerable<classes_teachers_allocated> GetTeacherTimeTable(long teacherid, DayOfWeek day, int year)
        {
            return
                db.classes_teachers_allocateds.Where(x => x.teacherid == teacherid && x.day == (int)day && x.year == year).
                    OrderBy(x => x.time_start);
        }
    }
}
