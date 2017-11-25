using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public IEnumerable<user> GetStudentsByGuardian(long guardianid)
        {
            return db.students_guardians.Where(x => x.parentid == guardianid).Select(x => x.user);
        }
    }
}
