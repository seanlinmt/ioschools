using System;
using System.Collections.Generic;
using System.Linq;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;

namespace ioschools.DB.repository
{
    public interface IRepository
    {
        void Save();
        
        // attendance
        void DeleteAttendance(attendance att);
        attendance GetClassAttendance(long studentid, int classid, DateTime date);
        attendance GetEcaAttendance(long studentid, long ecaid, DateTime date);
        attendance GetAttendance(long id);
        attendance_term GetAttendanceTerm(int schoolid, int termid, int year);

        // blog
        void AddBlog(blog blog);
        long AddBlogFile(blog_file file);
        void DeleteBlog(long id);
        void DeleteBlogFile(long id);
        void DeleteBlogPhoto(long id);
        blog GetBlog(long id);
        blog_image GetBlogPhoto(long id);
        blog_file GetBlogFile(long id);
        IEnumerable<blog> GetBlogs();
        IEnumerable<blog> GetBlogs(BlogType? type, int? year);
        void UpdateBlogFiles(long id, string[] files);
        void UpdateBlogImages(long blogid, IEnumerable<string> photoids);

        // calendar
        IQueryable<calendar> GetCalendarEntries(int? year = null);

        // change logs
        IQueryable<changelog> GetChangeLogs();

        // classes
        void DeleteStudentAllocatedClass(long id);
        void DeleteTeacherAllocatedClass(long id);
        classes_teachers_allocated GetAllocatedTeacherClass(long id);
        IQueryable<school_class> GetSchoolClasses();
        IEnumerable<school_class> GetClassesImTeachingThisSubject(long teacherid, long subjectid, int year);

        // discipline
        void DeleteDiscipline(students_discipline discipline);
        students_discipline GetDiscipline(long id);
        IEnumerable<students_discipline> GetDisciplines();

        // eca
        void AddEca(eca eca);
        void DeleteEca(long id);
        IEnumerable<eca> GetEcas(int? schoolid);
        void DeleteStudentEca(long id);
        eca GetEca(long id);
        eca_student GetStudentEca(long id);
        IEnumerable<eca> GetViewableEcas(Schools? userschool);

        // employment
        employment GetEmploymentPeriod(long id);

        // enrolment
        void AddRegistration(registration registration);
        void DeleteRegistrationNotificationByUser(long userid);
        void DeleteRegistraion(registration registration);
        registration GetRegistration(long id);
        IQueryable<registration> GetRegistration(int? school, RegistrationStatus? status, int? year, int? classyear);
        IEnumerable<registration_notification> GetRegistrationNotifications();

        // exam
        void AddExam(exam exam);
        void AddExamMark(exam_mark emark);
        void AddExamTemplate(exam_template template);
        string CanModifyExamMark(long modifier_userid, long subjectid, long studentid, int year);
        void DeleteExamMark(exam_mark emark);
        void DeleteExamTemplate(int id);
        void DeleteExamTemplateSubject(int id);
        exam GetExam(long id);
        exam_mark GetExamMark(long examid, long studentid, long subjectid);
        exam_template GetExamTemplate(int id);
        IQueryable<exam_template> GetExamTemplates(long viewerid);
        IQueryable<exam> GetExams(int? school, int? form, int? year);

        // fees
        IQueryable<fee> GetFees();

        // homework
        void AddHomework(homework homework);
        void AddHomeworkFile(homework_file file);
        void DeleteHomework(long id);
        void DeleteHomeworkClasses(long homeworkid);
        void DeleteHomeworkFile(homework_file file);
        homework GetHomework(long id);
        IEnumerable<homework> GetHomeworks(long creator, int year);
        IEnumerable<homework> GetHomeworksByStudent(long studentid, int year);
        homework_file GetHomeworkFile(long id);
        
        // image
        long AddBlogImage(blog_image img);
        long AddUserImage(user_image image);
        void DeleteUserImage(long id);
        
        // mail
        IQueryable<mail> GetMails();
        void DeleteMail(mail entry);

        // schools
        void AddSchoolSubject(subject subject);
        IQueryable<school> GetSchools();
        IQueryable<school> GetSchools(Schools? myschool);
        IQueryable<school_year> GetSchoolYears();
        IEnumerable<school_term> GetSchoolTerms();

        // subjects
        void DeleteSchoolSubject(long id);
        subject GetSchoolSubject(long id);
        IQueryable<subject> GetSchoolSubjects(int? schoolid);
        IEnumerable<subject> GetSubjectsImTeaching(long teacherid, int year);
        IEnumerable<subject_teacher> GetSubjectTeachers(long teacherid, int year);

        // user
        user GetUser(long id);
        IQueryable<user> GetUsers();
        IEnumerable<user> GetActiveUsers();
        long GetUserDiskUsage(long sessionid);
        void AddUser(user u);
        void DeleteStudentOrGuardian(long id);
        IEnumerable<user> GetStudentsByGuardian(long guardianid);
        IQueryable<user> GetUsers(long? viewerid, UserAuth viewer_auth, int? school, int? schoolClass, UserGroup? group, string discipline,
            AttendanceStatus? attendanceStatus, string attendanceDate, int year, int? eca, bool active = true, bool hasIssues = false);
        IQueryable<user> GetStudentsByPhysicalClass(long classid, int year);
        void DeleteUser(long id, long executer);
        user GetActiveUserLogin(string email, string password);
        user GetUserByHash(string hash);
        user GetUserByNewNRIC(string nric);
        bool IsStudentInMyClass(long myid, long studentid, int year);
        
        // time table
        classes_teachers_allocated GetClassPeriod(int year, int day, int schoolid, int classid, TimeSpan periodStart, TimeSpan periodEnd);
        IEnumerable<classes_teachers_allocated> GetStudentTimeTable(long userid, DayOfWeek day, int year);
        IEnumerable<classes_teachers_allocated> GetTeacherTimeTable(long teacherid, DayOfWeek day, int year);


        IEnumerable<string> GetExamSubjects(long[] subjectids);
        void AddChangeLog(changelog change);
        IEnumerable<int> GetOperationalYears();

        page GetPage(int typeid);
        void AddPage(page page);


        
    }
}
