using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public homework GetHomework(long id)
        {
            return db.homeworks.SingleOrDefault(x => x.id == id);
        }

        public IEnumerable<homework> GetHomeworks(long creator, int year)
        {
            return db.homeworks.Where(x => x.creator == creator && x.created.Year == year);
        }

        public homework_file GetHomeworkFile(long id)
        {
            return db.homework_files.SingleOrDefault(x => x.id == id);
        }

        public IEnumerable<homework> GetHomeworksByStudent(long studentid, int year)
        {
            var studentclass = db.classes_students_allocateds.SingleOrDefault(x => x.studentid == studentid && x.year == year);
            if (studentclass != null)
            {
                return db.homework_students
                    .Where(x => x.classid == studentclass.classid && x.studentid == studentid)
                    .Select(x => x.homework);
            }
            return Enumerable.Empty<homework>();
        }

        public void AddHomework(homework homework)
        {
            db.homeworks.InsertOnSubmit(homework);
            Save();
        }

        public void AddHomeworkFile(homework_file file)
        {
            db.homework_files.InsertOnSubmit(file);
            Save();
        }

        public void DeleteHomework(long id)
        {
            var exist = db.homeworks.SingleOrDefault(x => x.id == id);
            if (exist != null)
            {
                db.homework_classes.DeleteAllOnSubmit(exist.homework_classes);
                db.homework_files.DeleteAllOnSubmit(exist.homework_files);
                db.homework_answers.DeleteAllOnSubmit(exist.homework_answers);
                db.homework_students.DeleteAllOnSubmit(exist.homework_students);
                db.homeworks.DeleteOnSubmit(exist);
                Save();
            }
        }

        public void DeleteHomeworkClasses(long homeworkid)
        {
            var classes = db.homework_classes.Where(x => x.homeworkid == homeworkid);
            db.homework_classes.DeleteAllOnSubmit(classes);
            Save();
        }

        public void DeleteHomeworkFile(homework_file file)
        {
            db.homework_files.DeleteOnSubmit(file);
            Save();
        }
    }
}
