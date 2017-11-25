using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void DeleteStudentAllocatedClass(long id)
        {
            var exist = db.classes_students_allocateds.Where(x => x.id == id).SingleOrDefault();
            if (exist != null)
            {
                db.classes_students_allocateds.DeleteOnSubmit(exist);
                Save();
            }
        }

        public void DeleteTeacherAllocatedClass(long id)
        {
            var exist = db.classes_teachers_allocateds.Where(x => x.id == id).SingleOrDefault();
            if (exist != null)
            {
                db.classes_teachers_allocateds.DeleteOnSubmit(exist);
                Save();
            }
        }

        public classes_teachers_allocated GetAllocatedTeacherClass(long id)
        {
            return db.classes_teachers_allocateds.Where(x => x.id == id).SingleOrDefault();
        }

        public IQueryable<school_class> GetSchoolClasses()
        {
            return db.school_classes;
        }

        public IEnumerable<school_class> GetClassesImTeachingThisSubject(long teacherid, long subjectid, int year)
        {
            return db.classes_teachers_allocateds.Where(x => x.year == year &&
                                                      x.teacherid == teacherid &&
                                                      x.subjectid.HasValue && x.subjectid.Value == subjectid)
                .Select(x => x.school_class).Distinct();
        }
    }
}
