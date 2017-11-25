using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void AddSchoolSubject(subject subject)
        {
            db.subjects.InsertOnSubmit(subject);
        }

        public void DeleteSchoolSubject(long id)
        {
            var subject = GetSchoolSubject(id);

            if (subject != null)
            {
                db.subjects.DeleteOnSubmit(subject);
                Save();
            }
        }

        public subject GetSchoolSubject(long id)
        {
            return db.subjects.SingleOrDefault(x => x.id == id);
        }

        public IQueryable<subject> GetSchoolSubjects(int? schoolid)
        {
            if (schoolid.HasValue)
            {
                return db.subjects.Where(x => x.schoolid == schoolid.Value);
            }

            return db.subjects;
        }

        public IEnumerable<subject> GetSubjectsImTeaching(long teacherid, int year)
        {
            return
                db.classes_teachers_allocateds.Where(x => x.year == year && x.teacherid == teacherid)
                    .Select(x => x.subject).Distinct();
        }

        public IEnumerable<subject_teacher> GetSubjectTeachers(long teacherid, int year)
        {
            return db.subject_teachers
                .Where(x => x.teacherid == teacherid && x.year == year);
        }
    }
}
