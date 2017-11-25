using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;
using ioschools.Data.User;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void DeleteAttendance(attendance att)
        {
            db.attendances.DeleteOnSubmit(att);
        }

        public attendance GetClassAttendance(long studentid, int classid, DateTime date)
        {
            return db.attendances.SingleOrDefault(x => x.classid == classid && x.date == date && x.studentid == studentid);
        }

        public attendance GetEcaAttendance(long studentid, long ecaid, DateTime date)
        {
            return db.attendances.SingleOrDefault(x => x.ecaid == ecaid && x.date == date && x.studentid == studentid);
        }

        public attendance GetAttendance(long id)
        {
            return db.attendances.SingleOrDefault(x => x.id == id);
        }

        public IQueryable<user> GetStudentsByPhysicalClass(long classid, int year)
        {
            return db.classes_students_allocateds.Where(x => x.classid == classid && x.year == year).Select(y => y.user);
        }
    }
}
