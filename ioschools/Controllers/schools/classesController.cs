using System;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Extensions.Models;
using ioschools.Library.Helpers;
using ioschools.Models.school;
using ioschools.Models.user;
using clearpixels.Logging;
using ioschools.DB;

namespace ioschools.Controllers.schools
{
    public class classesController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Add(int schoolid)
        {
            var viewmodel = new SchoolClass();
            viewmodel.schoolyearList = db.school_years
                .Where(x => x.schoolid == schoolid)
                .OrderBy(x => x.name)
                .Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() });

            var classes = new[] { new GroupSelectListItem() { Text = "None", Value = "" } }.AsEnumerable();
            if (db.school_classes.Any())
            {
                classes = classes.Union(db.school_classes
                .OrderBy(x => x.schoolid)
                .ThenBy(x => x.name)
                .Select(x => new GroupSelectListItem()
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                    Group = x.school.name
                }));
            }
            viewmodel.nextClassList = classes.ToArray();

            return View("Edit", viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult AdminContent(int id)
        {
            var school = repository.GetSchools().Single(x => x.id == id);

            var viewmodel = new AdminSchoolClassesViewModel();
            viewmodel.schoolid = id;
            viewmodel.schoolname = school.name;
            viewmodel.schoolYears = school.school_years
                .OrderBy(x => x.name)
                .ToModel();

            viewmodel.classes = school.school_classes
                .OrderBy(x => x.school_year.name)
                .ThenBy(x => x.name)
                .ToModel();

            return View(viewmodel);
        }


        public ActionResult AssignToTeachers()
        {
            var viewmodel =
                new[] { new SelectListItem() { Text = "select school", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        public ActionResult AssignToStudents()
        {
            var viewmodel =
                new[] { new SelectListItem() { Text = "select school", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Delete(int id)
        {
            var single = db.school_classes.Single(x => x.id == id);

            try
            {
                db.school_classes.DeleteOnSubmit(single);
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return Json("Failed to delete entry".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = (Permission.USERS_EDIT_STUDENTS))]
        public ActionResult DetachStudentClass(long id)
        {
            try
            {
                repository.DeleteStudentAllocatedClass(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Class removed successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = (Permission.USERS_EDIT_STAFF | Permission.USERS_EDIT_OWN))]
        public ActionResult DetachTeacherClass(long id)
        {
            var klass = repository.GetAllocatedTeacherClass(id);
            if (klass == null)
            {
                return Json("Unable to find entry".ToJsonFail());
            }

            var canedit = klass.user.GetCanEdit(sessionid.Value, auth);
            if (!canedit)
            {
                return SendJsonNoPermission();
            }

            try
            {
                repository.DeleteTeacherAllocatedClass(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Class removed successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Edit(int id)
        {
            var result = repository.GetSchoolClasses().Single(x => x.id == id);
            var viewmodel = result.ToModel();
            if (db.school_years.Any())
            {
                viewmodel.schoolyearList = db.school_years
                .Where(x => x.schoolid == result.schoolid)
                .OrderBy(x => x.name)
                .Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString(), Selected = x.id == result.schoolyearid });

            }

            var classes = new[] {new GroupSelectListItem() {Text = "None", Value = ""}}.AsEnumerable();
            if (db.school_classes.Any())
            {
                classes = classes.Union(db.school_classes
                .OrderBy(x => x.schoolid)
                .ThenBy(x => x.name)
                .Select(x => new GroupSelectListItem()
                                 {
                                     Text = x.name, 
                                     Value = x.id.ToString(),
                                     Selected = x.name == viewmodel.nextClass,
                                     Group = x.school.name
                                 }));
            }
            viewmodel.nextClassList = classes.ToArray();

            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Rows(int? id, int? schoolid)
        {
            if (id.HasValue)
            {
                return View(new[] { db.school_classes.Single(x => x.id == id.Value).ToModel() });
            }

            return View(db.school_classes.Where(x => x.schoolid == schoolid.Value)
                            .OrderBy(x => x.school_year.name)
                            .ThenBy(x => x.name)
                            .ToModel());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult Save(int? id, string name, int year, int? next, int schoolid)
        {
            var single = new school_class();
            if (id.HasValue)
            {
                single = db.school_classes.Single(x => x.id == id);
            }
            else
            {
                db.school_classes.InsertOnSubmit(single);
            }
            single.name = name;
            single.schoolyearid = year;
            single.nextclass = next;
            single.schoolid = schoolid;

            repository.Save();

            var viewmodel = "Entry saved successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("Rows", new[]{single.ToModel()});

            return Json(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Selector()
        {
            return View();
        }
    }
}
