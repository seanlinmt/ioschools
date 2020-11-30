using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models;
using ioschools.Models.eca;
using ioschools.Models.subject;
using ioschools.Models.subject.JSON;
using ioschools.Models.subject.viewmodels;
using ioschools.Models.user;
using clearpixels.Logging;
using ioschools.DB;

namespace ioschools.Controllers.subjects
{
    public class subjectsController : baseController
    {
        public ActionResult Selector()
        {
            return View();
        }

        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Delete(long id)
        {
            try
            {
                repository.DeleteSchoolSubject(id);
                //HttpResponse.RemoveOutputCacheItem(Url.Action("list"));
            }
            catch (Exception ex)
            {
                return Json("Unable to delete subject. Subject in use.".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Edit(long? id)
        {
            var viewmodel = new SubjectRow();
            if (id.HasValue)
            {
                viewmodel.subject = repository.GetSchoolSubject(id.Value).ToModel();
                viewmodel.schoolList =
                    repository.GetSchools().Select(
                        x =>
                        new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString(),
                            Selected = (x.id == viewmodel.subject.schoolid)
                        });
            }
            else
            {
                viewmodel.schoolList =
                    repository.GetSchools().Select(
                        x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        });
            }

            return View(viewmodel);
        }

        /// <summary>
        /// save new or update existing eca entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="school"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Save(int? id, int school, string name)
        {
            var subject = new subject();
            if (id.HasValue)
            {
                subject = repository.GetSchoolSubject(id.Value);
            }
            subject.name = name;
            subject.schoolid = school;

            if (!id.HasValue)
            {
                repository.AddSchoolSubject(subject);
            }
            try
            {
                repository.Save();
                //HttpResponse.RemoveOutputCacheItem(Url.Action("list", new { id = school }));
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }
            return Json("Entry saved successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult TeachersEdit(long subject, int year, int school)
        {
            // get active teachers
            var teachers = repository.GetUsers()
                .Where(x => (x.usergroup == (int)UserGroup.TEACHER || x.usergroup == (int)UserGroup.HEAD) &&
                    x.schoolid.HasValue && x.schoolid.Value == school &&
                    x.employments.Count != 0 &&
                    x.employments.Any(y => y.start_date.HasValue && y.start_date.Value.Year <= year &&
                                        (!y.end_date.HasValue || y.end_date.Value.Year >= year)))
                                        .OrderBy(x => x.name)
                                        .Select(x => new IdName()
                                        {
                                            id = x.id.ToString(),
                                            name = x.ToName(false)
                                        });

            var s = repository.GetSchoolSubject(subject);

            var viewmodel = new SubjectTeacher();
            viewmodel.subjectid = s.id;
            viewmodel.subjectname = s.name;
            
            var subjects = db.subject_teachers.Where(x => x.subjectid == subject);

            if (!subjects.Any())
            {
                // nothing
                var subjectentry = new SubjectTeacherEntry();
                subjectentry.teachers = teachers.Select(x => new SelectListItem()
                                                                 {
                                                                     Text = x.name,
                                                                     Value = x.id
                                                                 });
                subjectentry.classes = db.school_classes
                    .Where(x => x.schoolid == school)
                    .OrderBy(x => x.name)
                    .Select(x => new SelectListItem()
                    {
                        Text = x.name,
                        Value = x.id.ToString()
                    });
                viewmodel.teachers.Add(subjectentry);
            }
            else
            {
                foreach (var teacher in subjects.GroupBy(x => x.user))
                {
                    var subjectentry = new SubjectTeacherEntry();
                    subjectentry.classes = db.school_classes
                        .Where(x => x.schoolid == school)
                        .OrderBy(x => x.name)
                        .Select(x => new SelectListItem()
                                         {
                                             Text = x.name,
                                             Value = x.id.ToString()
                                         });
                    IGrouping<user, subject_teacher> teacher1 = teacher;
                    subjectentry.teachers = teachers.Select(x => new SelectListItem()
                    {
                        Text = x.name,
                        Value = x.id,
                        Selected = x.id == teacher1.Key.id.ToString()
                    });
                    foreach (var klass in teacher.OrderBy(x => x.school_class.name))
                    {
                        subjectentry.AllocatedClasses.Add(new IdName(klass.classid, klass.school_class.name));
                    }
                    viewmodel.teachers.Add(subjectentry);
                }
            }

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        [JsonFilter(Param = "data", RootType = typeof(SubjectTeacherJSONCollection))]
        public ActionResult TeachersSave(SubjectTeacherJSONCollection data)
        {
            var existing = db.subject_teachers.Where(x => x.subjectid == data.subjectid && x.year == data.year);
            
            db.subject_teachers.DeleteAllOnSubmit(existing);

            foreach (var entry in data.teachers)
            {
                foreach (var classentry in entry.classes)
                {
                    var st = new subject_teacher
                                 {
                                     teacherid = entry.id,
                                     subjectid = data.subjectid,
                                     year = data.year,
                                     classid = classentry
                                 };
                    db.subject_teachers.InsertOnSubmit(st);
                }
            }

            repository.Save();

            return Json("Subject successfully updated".ToJsonOKMessage());
        }

    }
}
