using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.FileUploader;
using ioschools.Library.Helpers;
using ioschools.Library.email;
using ioschools.Models.homework.viewmodels;
using ioschools.Models.notifications;
using ioschools.Models.user;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.File;
using ioschools.Models;
using ioschools.Models.homework;

namespace ioschools.Controllers.elearning
{
    public class homeworkController : baseController
    {
        [HttpPost]
        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult Delete(long id)
        {
            try
            {
                var homework = repository.GetHomework(id);
                if (homework != null)
                {
                    foreach (var file in homework.homework_files)
                    {
                        FileHandler.Delete(file.url);
                    }
                }
                repository.DeleteHomework(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Homework deleted".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult Edit(long id)
        {
            var viewmodel = repository
                .GetHomeworks(sessionid.Value, Utility.GetDBDate().Year)
                .Single(x => x.id == id)
                .ToModel(true, Utility.GetDBDate().Year);

            viewmodel.editmode = true;

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult StudentRows(long? subject)
        {
            var year = DateTime.Now.Year;

            var homeworkStudents = db.homework_students.Where(x => x.studentid == sessionid.Value && x.homework.created.Year == year);

            if (subject.HasValue)
            {
                homeworkStudents = homeworkStudents.Where(x => x.homework.subjectid == subject);
            }

            var viewmodel = homeworkStudents.OrderByDescending(x => x.id).ToModel(true);
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult TeacherRows(int year, long? subject)
        {
            var homeworks = repository.GetHomeworks(sessionid.Value, year);

            if (subject.HasValue)
            {
                homeworks = homeworks.Where(x => x.subjectid == subject);
            }

            var viewmodel = homeworks.OrderByDescending(x => x.id).ToModel(true, year);
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Index()
        {
            var date = Utility.GetDBDate();
            var viewmodel = new HomeworkViewModel(baseviewmodel);
            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                if (!auth.perms.HasFlag(Permission.HOMEWORK_CREATE))
                {
                    return ReturnNoPermissionView();
                }
                viewmodel.editable = true;
                // get subjects teaching for current year
                viewmodel.subjects = new[] { new SelectListItem() { Text = "All", Value = "" } }.Union(
                    repository.GetHomeworks(sessionid.Value, date.Year)
                    .Select(x => x.subject)
                            .Distinct()
                            .Select(x => new SelectListItem()
                            {
                                Text = x.name,
                                Value = x.id.ToString()
                            }));

                viewmodel.homeworks = repository.GetHomeworks(sessionid.Value, date.Year)
                                                .OrderByDescending(x => x.id)
                                                .ToModel(true, date.Year);

                // handle disk space calculations
                viewmodel.DiskspaceUsed = viewmodel.homeworks.Sum(x => x.totalSize);
                viewmodel.DiskspaceLeft = Constants.MAX_DISK_SIZE - viewmodel.DiskspaceUsed;

            }
            else
            {
                viewmodel.editable = false;
            }
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult HomeworkAnswers(long id)
        {
            var homework = repository.GetHomework(id);
            if (homework == null)
            {
                return Json("Homework not found".ToJsonFail());
            }

            var viewmodel = new List<HomeworkAnswer>();

            foreach (var student in homework.homework_students)
            {
                var answer = new HomeworkAnswer();
                answer.studentname = student.user.ToName();
                answer.studentid = student.user.id;
                answer.classname = student.school_class.name;
                answer.files = student.homework_answers.Select(x => new HomeworkAnswerFile()
                                                                        {
                                                                            url = x.url,
                                                                            name = x.filename,
                                                                            created =
                                                                                x.created.ToString(
                                                                                    Constants.DATETIME_FULL)
                                                                        });
                viewmodel.Add(answer);
            }

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult HomeworkContent(long id, int year)
        {
            var usr = repository.GetUser(id);

            var viewmodel = Enumerable.Empty<HomeworkStudent>();
            var allocated = usr.classes_students_allocateds.SingleOrDefault(x => x.year == year);
            if (allocated != null)
            {
                viewmodel = allocated.school_class
                                .homework_students
                                .Where(x => x.homework.created.Year == year && x.studentid == id)
                                .OrderByDescending(x => x.homework.created)
                                .ToModel(false);
            }

            return View(viewmodel);
        }

        [HttpPost]
        [JsonFilter(RootType = typeof(HomeworkJSON),Param = "data")]
        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult Save(HomeworkJSON data)
        {
            var date = Utility.GetDBDate();
            var homework = new homework();

            if (!string.IsNullOrEmpty(data.id))
            {
                homework = repository.GetHomeworks(sessionid.Value, date.Year).Single(x => x.id.ToString() == data.id);

                repository.DeleteHomeworkClasses(homework.id);
            }

            homework.title = data.title;
            homework.message = data.message;
            homework.subjectid = data.subject;
            homework.created = date;
            homework.creator = sessionid.Value;
            homework.notifyme = data.notifyme;

            // need to save first to get homework id
            repository.Save();

            foreach (var s in data.students)
            {
                var student = new homework_student();
                student.studentid = s;
                student.classid = db.classes_students_allocateds.First(x => x.year == date.Year && x.studentid == s).classid;
                homework.homework_students.Add(student);
            }

            // now to update all relevant homework files
            foreach (var file in data.files)
            {
                var hfile = repository.GetHomeworkFile(long.Parse(file.id));
                hfile.homeworkid = homework.id;
                homework.homework_files.Add(hfile);
            }
            if (string.IsNullOrEmpty(data.id))
            {
                repository.AddHomework(homework);
            }

            repository.Save();

            var viewmodel = "Homework saved successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("Single", homework.ToModel(true,homework.created.Year));

            return Json(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Single(long id)
        {
            var viewmodel = repository.GetHomework(id).ToModel(true, DateTime.Now.Year);

            return View(viewmodel);
        }


        /// <summary>
        /// used for uploading answers by students
        /// </summary>
        /// <param name="id"></param>
        /// <param name="qqfile"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult UploadAnswer(long id, string qqfile)
        {
            var length = long.Parse(Request.Params["CONTENT_LENGTH"]);

            var filename = Path.GetFileNameWithoutExtension(qqfile);
            var ext = Path.GetExtension(qqfile);

            var uploader = new FileHandler(filename.ToSafeUrl() + ext, UploaderType.HOMEWORK, sessionid);
            bool ok = uploader.Save(Request.InputStream);

            if (!ok)
            {
                return Json("An error has occurred. Unable to save file".ToJsonFail());
            }

            // save to database
            var homeworkstudent = db.homework_students.SingleOrDefault(x => x.studentid == sessionid.Value && x.id == id);

            var answer = new homework_answer();
            answer.created = DateTime.Now;
            answer.url = uploader.url;
            answer.filename = qqfile;
            answer.size = uploader.size;
            answer.homeworkid = homeworkstudent.homeworkid;
            homeworkstudent.homework_answers.Add(answer);

            repository.Save();

            // notify teacher?
            if (homeworkstudent.homework.notifyme)
            {
                var viewmodel = new NotificationSendViewModel();
                var studentname = homeworkstudent.user.ToName();
                viewmodel.message =
                    string.Format(
                        "An answer has been uploaded by <a href='http://wwww.ioschools.edu.my/users/{0}'>{1}</a>",
                        homeworkstudent.studentid, studentname);
                viewmodel.message += string.Format("<br/> To view your homeworks. <a href='http://wwww.ioschools.edu.my/homework'>Please follow this link</a>.");
                
                var teacher = homeworkstudent.homework.user;

                if (!string.IsNullOrEmpty(teacher.email))
                {
                    viewmodel.receiver = teacher.ToName();
                    this.SendEmailNow(
                            EmailViewType.HOMEWORK_NOTIFICATION,
                            viewmodel,
                            string.Format("Homework {0}: Answer uploaded by {1}", homeworkstudent.homework.title, studentname),
                            teacher.email,
                            viewmodel.receiver);
                }
            }

            var retVal = new IdName(answer.id, uploader.filename);

            return Json(retVal.ToJsonOKData());
        }


        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult New()
        {
            // don't limit by subject teachers
            var usr = repository.GetUser(sessionid.Value);
            if (!usr.schoolid.HasValue)
            {
                return new EmptyResult();
            }

            var viewmodel = new Homework()
            {
                subjectList = usr.school.subjects
                                .Select(x => new SelectListItem()
                                {
                                    Text = x.name,
                                    Value = x.id.ToString()
                                }),
                classList = usr.school.school_classes
                                .Select(x => new SelectListItem()
                                {
                                    Text = x.name,
                                    Value = x.id.ToString()
                                }),
            };
            return View("Edit", viewmodel);
        }
    }
}
