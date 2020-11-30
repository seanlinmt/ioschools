using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.user.staff;
using ioschools.DB;

namespace ioschools.Controllers.staff
{
    [PermissionFilter(perm = Permission.USERS_EDIT_STAFF)]
    public class staffController : baseController
    {
        [HttpPost]
        public ActionResult DeleteEmploymentPeriod(long id)
        {
            var exist = repository.GetEmploymentPeriod(id);

            if (exist == null)
            {
                return Json("Entry not found".ToJsonFail());
            }

            db.employments.DeleteOnSubmit(exist);

            repository.Save();

            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        public ActionResult EmploymentEditRow(long? id)
        {
            var viewmodel = new EmploymentPeriod();
            if (id.HasValue)
            {
                var existing = repository.GetEmploymentPeriod(id.Value);
                if (existing != null)
                {
                    viewmodel.id = existing.id.ToString();
                    viewmodel.startDate = existing.start_date.HasValue
                                              ? existing.start_date.Value.ToString(Constants.DATETIME_SHORT_DATE)
                                              : "";
                    viewmodel.endDate = existing.end_date.HasValue
                                            ? existing.end_date.Value.ToString(Constants.DATETIME_SHORT_DATE)
                                            : "";
                    viewmodel.remarks = existing.remarks;
                }
            }
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult EmploymentRow(long id)
        {
            var employment = repository.GetEmploymentPeriod(id);

            return View(new[] { employment.ToModel() });
        }

        [HttpPost]
        public ActionResult EmploymentSave(long? id, DateTime? start, DateTime? end, string remarks, long staffid)
        {
            var employment = new employment();
            if (id.HasValue)
            {
                employment = repository.GetEmploymentPeriod(id.Value);
            }
            employment.start_date = start;
            employment.end_date = end;
            employment.remarks = remarks;

            if (!id.HasValue)
            {
                employment.userid = staffid;
                db.employments.InsertOnSubmit(employment);
            }

            repository.Save();

            var viewmodel = "Entry updated successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("EmploymentRow", new[] { employment.ToModel() });

            return Json(viewmodel);
        }

    }
}
