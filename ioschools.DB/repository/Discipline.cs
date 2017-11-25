using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void DeleteDiscipline(students_discipline discipline)
        {
            db.students_disciplines.DeleteOnSubmit(discipline);
        }

        public students_discipline GetDiscipline(long id)
        {
            return db.students_disciplines.SingleOrDefault(x => x.id == id);
        }

        public IEnumerable<students_discipline> GetDisciplines()
        {
            return db.students_disciplines;
        }
    }
}
