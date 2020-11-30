using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.schedule;

namespace ioschools.Controllers.schedule
{
    [PermissionFilter(perm = Permission.NONE)]
    public class scheduleController : baseController
    {
        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

        public ActionResult ScheduleContent()
        {
            var date = Utility.GetDBDate();
            IEnumerable<Schedule> viewmodel;
            if (UserSuperGroups.STAFF.HasFlag(auth.group))
            {
                viewmodel = repository.GetTeacherTimeTable(sessionid.Value, date.DayOfWeek, date.Year).ToSchedule();
            }
            else if (auth.group == UserGroup.STUDENT)
            {
                viewmodel = repository.GetStudentTimeTable(sessionid.Value, date.DayOfWeek, date.Year).ToSchedule();
            }
            else
            {
                viewmodel = Enumerable.Empty<Schedule>();
            }

            return View(viewmodel);
        }
    }
}
