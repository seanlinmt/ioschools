using System;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.dashboard;
using ioschools.Models.discipline;
using ioschools.Models.homework.viewmodels;
using ioschools.Models.stats;
using ioschools.Models.user;
using ioschools.Models.user.student;
using ioschools.Models.exam;

namespace ioschools.Controllers.dashboard
{
    [PermissionFilter(perm = Permission.NONE)]
    public class dashboardController : baseController
    {
        [NoCache]
        public ActionResult Index(long? id)
        {
            if (!sessionid.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            var usr = repository.GetUser(sessionid.Value);
            baseviewmodel.notifications = usr.ToNotification().ToJson();
            var date = Utility.GetDBDate();

            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                var viewmodel = new DashboardViewModel(baseviewmodel);
                viewmodel.yearList = repository.GetOperationalYears()
                                                .OrderBy(x => x)
                                                .Select(x => new SelectListItem()
                                                {
                                                    Text = x.ToString(),
                                                    Value = x.ToString(),
                                                    Selected = (x == date.Year)
                                                });

                return View(viewmodel);
            }

            if (auth.group == UserGroup.STUDENT)
            {
                var sview = new StudentViewModel(baseviewmodel)
                {
                    exam = new ExamStudentViewModel(usr, usr.exam_marks.AsQueryable(), date.Year),
                    discipline =
                        new DisciplineViewModel(usr, sessionid.Value, UserGroup.STUDENT, date.Year),
                    homework =
                        new HomeworkStudentViewModel(usr.id,
                                                     usr.classes_students_allocateds.AsQueryable(),
                                                     date.Year,
                                                     true)
                };

                sview.homework.subjects = new[] { new SelectListItem() { Text = "All", Value = "" } }
                    .Union(usr.homework_students
                    .Where(x => x.homework.created.Year == date.Year)
                    .Select(x => x.homework.subject)
                    .Distinct()
                    .Select(x => new SelectListItem() { Value = x.id.ToString(), Text = x.name }));

                return View("student", sview);
            }

            if (auth.group == UserGroup.GUARDIAN)
            {
                var gview = new GuardianViewModel(baseviewmodel);
                gview.students = repository.GetStudentsByGuardian(sessionid.Value).ToModel(sessionid.Value, auth, date.Year);

                return View("parent", gview);
            }

            return new EmptyResult();
        }

        public ActionResult Navigation()
        {
            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                return View("NavigationStaff");
            }

            if (auth.group == UserGroup.STUDENT)
            {
                return View("NavigationStudent");
            }

            if (auth.group == UserGroup.GUARDIAN)
            {
                return View("NavigationParents");
            }

            return new EmptyResult();
        }


        [HttpGet]
        public ActionResult Statistics()
        {
            if (!UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                return ReturnNoPermissionView();
            }
            var viewmodel = new StatsViewModel(baseviewmodel);

            var years = repository.GetDisciplines()
                .Select(x => x.created.Year)
                .Distinct()
                .Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString() });
            viewmodel.from_year = years;
            viewmodel.to_year = years;
            viewmodel.currentYear = DateTime.Now.Year;
            return View(viewmodel);
        }

    }
}
