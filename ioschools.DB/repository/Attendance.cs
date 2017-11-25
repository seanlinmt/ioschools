using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public attendance_term GetAttendanceTerm(int schoolid, int termid, int year)
        {
            return db.attendance_terms.Where(x => x.schoolid == schoolid && x.termid == termid && x.year == year).SingleOrDefault();
        }
    }
}
