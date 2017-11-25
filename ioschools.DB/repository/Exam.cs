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
        public void AddExam(exam exam)
        {
            db.exams.InsertOnSubmit(exam);
            Save();
        }

        public void AddExamMark(exam_mark emark)
        {
            db.exam_marks.InsertOnSubmit(emark);
        }

        public void AddExamTemplate(exam_template template)
        {
            db.exam_templates.InsertOnSubmit(template);
        }

        public string CanModifyExamMark(long modifier_userid, long exam_subjectid, long studentid, int year)
        {
            var subject = db.exam_subjects.Single(x => x.id == exam_subjectid);
            
            // allow non mapped subjects to be modified by anyone
            if (!subject.subjectid.HasValue)
            {
                return "";
            }
            var studentclasses = GetUser(studentid)
                .classes_students_allocateds
                .Where(x => x.year == year)
                .Select(x => x.classid).ToArray();

            var deniedString = db.subject_teachers.Where(x => x.teacherid == modifier_userid &&
                                                              x.subjectid == subject.subjectid.Value &&
                                                              studentclasses.Contains(x.classid) &&
                                                              x.year == year)
                                                .Select( x => string.Format("{0}({1})", x.subject.name,
                                                                                x.school_class.name))
                                                .FirstOrDefault();

            return deniedString;
        }


        public void DeleteExamMark(exam_mark emark)
        {
            db.exam_marks.DeleteOnSubmit(emark);
        }

        public void DeleteExamTemplate(int id)
        {
            
        }

        public void DeleteExamTemplateSubject(int id)
        {
            var exist = db.exam_template_subjects.Single(x => x.id == id);
            db.exam_template_subjects.DeleteOnSubmit(exist);
            Save();
        }

        public exam GetExam(long id)
        {
            return db.exams.SingleOrDefault(x => x.id == id);
        }

        public exam_mark GetExamMark(long examid, long studentid, long subjectid)
        {
            return
                db.exam_marks.SingleOrDefault(x => x.examid == examid && x.studentid == studentid && x.exam_subjectid == subjectid);
        }

        public exam_template GetExamTemplate(int id)
        {
            return db.exam_templates.SingleOrDefault(x => x.id == id);
        }

        public IQueryable<exam_template> GetExamTemplates(long viewerid)
        {
            return db.exam_templates.Where(x => !x.isprivate || (x.isprivate && x.creator.HasValue && x.creator.Value == viewerid));
        }

        public IQueryable<exam> GetExams(int? school, int? form, int? year)
        {
            var exams = db.exams.AsQueryable();
            if (school.HasValue)
            {
                exams =
                    exams.Where(x => x.schoolid == school.Value);
            }

            if (form.HasValue)
            {
                exams =
                    exams.SelectMany(x => x.exam_classes)
                        .Where(y => y.classid == form.Value)
                        .Select(y => y.exam)
                        .Distinct();
            }

            if (year.HasValue)
            {
                exams = exams.Where(x => x.year == year.Value);
            }
			
            return exams;
        }

        public IEnumerable<string> GetExamSubjects(long[] subjectids)
        {
            return db.exam_subjects.Where(x => subjectids.Contains(x.id)).Select(x => x.name);
        }
    }
}
