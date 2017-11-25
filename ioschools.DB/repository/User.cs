using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;
using clearpixels.Logging;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public user GetActiveUserLogin(string email, string password)
        {
            var usr = db.users.SingleOrDefault(x => x.email == email && (x.settings & (int)UserSettings.INACTIVE) == 0);
            if (usr != null && !string.IsNullOrEmpty(usr.passwordhash) && BCrypt.CheckPassword(email + password, usr.passwordhash))
            {
                return usr;
            }
            return null;
        }

        public user GetUser(long id)
        {
            return db.users.SingleOrDefault(x => x.id == id);
        }

        public IQueryable<user> GetUsers()
        {
            return db.users;
        }

        public IEnumerable<user> GetActiveUsers()
        {
            return db.users.Where(x => (x.settings & (int) UserSettings.INACTIVE) == 0);
        }

        public long GetUserDiskUsage(long sessionid)
        {
            var files = db.homeworks.Where(x => x.creator == sessionid).SelectMany(x => x.homework_files);
            if (!files.Any())
            {
                return 0;
            }
            return files.Sum(y => y.size);
        }

        public void AddUser(user u)
        {
            db.users.InsertOnSubmit(u);

            // do not save!!!! alot of other things depends on not saving
        }

        public void DeleteStudentOrGuardian(long id)
        {
            var exist = db.students_guardians.SingleOrDefault(x => x.id == id);
            if (exist != null)
            {
                db.students_guardians.DeleteOnSubmit(exist);
                Save();
            }
        }

        public employment GetEmploymentPeriod(long id)
        {
            return db.employments.SingleOrDefault(x => x.id == id);
        }

        public IQueryable<user> GetUsers(long? viewerid, UserAuth viewer_auth, int? school, int? schoolClass, UserGroup? group, string discipline, 
            AttendanceStatus? attendanceStatus, string attendanceDate, int year, int? eca, bool active = true, bool hasIssues = false)
        {
            /*
            var loadOptions = new DataLoadOptions();
            loadOptions.LoadWith<user>(x => x.user_image);
            loadOptions.LoadWith<user>(x => x.user_parents);
            loadOptions.LoadWith<user>(x => x.school);

            db.LoadOptions = loadOptions;
            db.DeferredLoadingEnabled = false;
            */
            var usrs = db.users.AsQueryable();

           if (group.HasValue)
            {
                usrs = usrs.Where(x => (x.usergroup & (int)group.Value) != 0);
            }

            if (active)
            {
                usrs = usrs.Where(x => (x.settings & (int)UserSettings.INACTIVE) == 0);
            }
            else
            {
                usrs = usrs.Where(x => (x.settings & (int)UserSettings.INACTIVE) != 0);
            }

            if (schoolClass.HasValue)
            {
                var teachers =
                    usrs.SelectMany(x => x.classes_teachers_allocateds)
                        .Where(y => y.classid == schoolClass.Value && y.year == year)
                        .Select(x => x.user);
                var students =
                    usrs.SelectMany(x => x.classes_students_allocateds)
                        .Where(y => y.classid == schoolClass.Value && y.year == year)
                        .Select(x => x.user);
                //var parents = students.SelectMany(x => x.students_guardians).Select(y => y.user1);
                usrs = teachers.Union(students).Distinct();
            }
            else if (school.HasValue)
            {
                usrs = usrs.Where(x => x.schoolid.HasValue && x.schoolid.Value == school);
            }
            else
            {
                var teachers =
                    usrs.SelectMany(x => x.classes_teachers_allocateds)
                        .Where(y => y.year == year)
                        .Select(x => x.user);
                var students =
                    usrs.SelectMany(x => x.classes_students_allocateds)
                        .Where(y => y.year == year)
                        .Select(x => x.user);
                //var parents = students.SelectMany(x => x.students_guardians).Select(y => y.user1);
                // now get others exclude teachers and students
                var others = usrs.Where(x => (x.usergroup & (int)(UserGroup.STUDENT | UserGroup.TEACHER)) == 0);
                usrs = teachers.Union(students).Union(others).Distinct();
            }

            if (eca.HasValue)
            {
                usrs = usrs.Where(
                    x =>
                    x.usergroup == (int)UserGroup.STUDENT &&
                    x.eca_students.Count(y => y.ecaid == eca.Value && y.year == year) != 0);
            }

            if (!string.IsNullOrEmpty(discipline))
            {
                // discipline can somehow end up as undefined
                try
                {
                    var sorter = new Sorter(discipline);
                    if (sorter.type == SorterType.LESSTHAN)
                    {
                        usrs = usrs.Where(x => x.students_disciplines.Where(w => w.created.Year == year).Sum(y => y.points) <= sorter.value && x.students_disciplines.Where(w => w.created.Year == year).Sum(y => y.points) != 0);
                    }
                    else
                    {
                        usrs = usrs.Where(x => x.students_disciplines.Where(w => w.created.Year == year).Sum(y => y.points) >= sorter.value && x.students_disciplines.Where(w => w.created.Year == year).Sum(y => y.points) != 0);
                    }
                }
                catch (Exception ex)
                {
                    Syslog.Write(ex);
                    Syslog.Write(ErrorLevel.WARNING, viewerid.HasValue?viewerid.Value.ToString():"");
                }
            }

            if (!string.IsNullOrEmpty(attendanceDate) && attendanceStatus.HasValue)
            {
                DateTime date;
                if (DateTime.TryParseExact(attendanceDate, Constants.DATEFORMAT_DATEPICKER, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    usrs = usrs.SelectMany(x => x.attendances).Where(x => x.date == date && x.status == attendanceStatus.Value.ToString()).Select(x => x.user);
                }
            }

            // filter here
            if (viewer_auth != null && viewerid.HasValue)
            {
                if (UserSuperGroups.STAFF.HasFlag(viewer_auth.group))
                {
                    var student = Enumerable.Empty<user>();
                    var parents = Enumerable.Empty<user>();
                    var staff = Enumerable.Empty<user>();

                    if ((viewer_auth.perms & (Permission.USERS_VIEW_STUDENTS | Permission.USERS_EDIT_STUDENTS)) != 0)
                    {
                        student = usrs.Where(x => x.usergroup == (int) UserGroup.STUDENT);
                    }

                    if ((viewer_auth.perms & (Permission.USERS_VIEW_PARENTS | Permission.USERS_EDIT_PARENTS)) != 0)
                    {
                        parents = usrs.Where(x => x.usergroup == (int)UserGroup.GUARDIAN);
                    }

                    if ((viewer_auth.perms & (Permission.USERS_VIEW_STAFF | Permission.USERS_EDIT_STAFF)) != 0)
                    {
                        staff = usrs.Where(x => (x.usergroup & (int)UserSuperGroups.STAFF) != 0);
                    }

                    usrs = student.Union(parents).Union(staff).AsQueryable();
                }
                else
                {
                    usrs = Enumerable.Empty<user>().AsQueryable();
                }
            }

            // do issue checking last as this is quite intensive
            if (hasIssues)
            {
                usrs = usrs.ToArray().Where(x => x.GetIssues(year, true) != UserIssue.NONE).AsQueryable();
            }

            return usrs;
        }

        public void DeleteUser(long id, long executer)
        {
            var usr = db.users.SingleOrDefault(x => x.id == id);
            if (usr == null)
            {
                return;
            }

            // delete blogs
            db.blogs.DeleteAllOnSubmit(usr.blogs);

            if (usr.usergroup == (int)UserGroup.STUDENT)
            {
                db.students_disciplines.DeleteAllOnSubmit(usr.students_disciplines);

                var allocatedclasses = db.classes_students_allocateds.Where(x => x.studentid == usr.id);
                db.classes_students_allocateds.DeleteAllOnSubmit(allocatedclasses);

                // delete guardian link
                db.students_guardians.DeleteAllOnSubmit(usr.students_guardians);
                db.students_guardians.DeleteAllOnSubmit(usr.students_guardians1);

                // delete attendances
                var attendances = db.attendances.Where(x => x.studentid == usr.id);
                db.attendances.DeleteAllOnSubmit(attendances);

                // delete eca
                db.eca_students.DeleteAllOnSubmit(usr.eca_students);

                // delete exam marks
                db.exam_marks.DeleteAllOnSubmit(usr.exam_marks);

                // delete siblings
                db.siblings.DeleteAllOnSubmit(usr.siblings);
                db.siblings.DeleteAllOnSubmit(usr.siblings1);

                // delete registrations
                db.registrations.DeleteAllOnSubmit(usr.registrations);
                
                // delete user files
                db.user_files.DeleteAllOnSubmit(usr.user_files);

                // delete user remarks
                db.students_remarks.DeleteAllOnSubmit(usr.students_remarks);
            }
            else
            {
                db.students_disciplines.DeleteAllOnSubmit(usr.students_disciplines1);

                // delete parent guardian link
                if (usr.user_parents != null)
                {
                    db.user_parents.DeleteOnSubmit(usr.user_parents);
                }
                db.students_guardians.DeleteAllOnSubmit(usr.students_guardians1);
                db.students_guardians.DeleteAllOnSubmit(usr.students_guardians);

                // staff
                if (usr.user_staffs != null)
                {
                    db.user_staffs.DeleteOnSubmit(usr.user_staffs);
                }

                // changelog
                db.changelogs.DeleteAllOnSubmit(usr.changelogs);

                // registrations
                db.registrations.DeleteAllOnSubmit(usr.registrations1.Union(usr.registrations2)); // the other one is for students only

                // teachers
                var allocated_teachers = db.classes_teachers_allocateds.Where(x => x.teacherid == usr.id);
                db.classes_teachers_allocateds.DeleteAllOnSubmit(allocated_teachers);

                // staff
                if (usr.employments != null)
                {
                    db.employments.DeleteAllOnSubmit(usr.employments);
                }

                // delete homework
                var homeworks = usr.homeworks;
                var homework_files = homeworks.SelectMany(x => x.homework_files);
                var homework_classes = homeworks.SelectMany(x => x.homework_classes);

                db.homework_classes.DeleteAllOnSubmit(homework_classes);
                db.homework_files.DeleteAllOnSubmit(homework_files);
                db.homeworks.DeleteAllOnSubmit(usr.homeworks);

                db.exam_classes.DeleteAllOnSubmit(usr.exams.SelectMany(x => x.exam_classes));
                db.exam_subjects.DeleteAllOnSubmit(usr.exams.SelectMany(x => x.exam_subjects));
                db.exam_marks.DeleteAllOnSubmit(usr.exams.SelectMany(x => x.exam_marks));
                db.exams.DeleteAllOnSubmit(usr.exams);
            }


            // log changes
            var change = new changelog
            {
                changes = string.Format("{0} deleted", usr.name),
                created = DateTime.Now,
                userid = executer
            };

            db.changelogs.InsertOnSubmit(change);
            db.users.DeleteOnSubmit(usr);
            db.SubmitChanges();
        }

        public user GetUserByHash(string hash)
        {
            return db.users.SingleOrDefault(x => x.passwordhash == hash);
        }

        public user GetUserByNewNRIC(string nric)
        {
            nric = nric.Replace("-", "");
            if (string.IsNullOrEmpty(nric))
            {
                return null;
            }
            return db.users.SingleOrDefault(x => x.nric_new == nric);
        }

        public bool IsStudentInMyClass(long myid, long studentid, int year)
        {
            var classesITeach = GetUser(myid).classes_teachers_allocateds.Where(x => x.year == year).Select(x => x.classid).Distinct().ToArray();

            var count = GetUser(studentid).classes_students_allocateds.Count(x => classesITeach.Contains(x.classid) && x.year == year);

            if (count == 0)
            {
                return false;
            }
            return true;
        }
    }
}
