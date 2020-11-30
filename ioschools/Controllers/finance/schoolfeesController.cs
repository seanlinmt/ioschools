using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.fees;
using ioschools.Models.fees.viewmodel;
using ioschools.Models.finance;

namespace ioschools.Controllers.finance
{
    public class schoolfeesController : baseController
    {
        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS)]
        public ActionResult DeleteStudent(long id)
        {
            var fee = db.fees.SingleOrDefault(x => x.id == id);

            if (fee == null)
            {
                return Json("Fee not found".ToJsonFail());
            }

            try
            {
                db.fees_reminders.DeleteAllOnSubmit(fee.fees_reminders);
                db.fees.DeleteOnSubmit(fee);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Fee deleted successfully".ToJsonOKMessage());

        }

        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS)]
        public ActionResult EditStudent(long id)
        {
            var viewmodel = new SchoolFeeStudentEdit();
            var fee = db.fees.Single(x => x.id == id);
            viewmodel.id = id.ToString();
            viewmodel.name = fee.name;
            viewmodel.amount = fee.amount;
            viewmodel.duedate = fee.duedateWithReminders;
            viewmodel.statusList = typeof(FeePaymentStatus).ToSelectList(false, null, null, fee.status);

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult OverdueFees(long id)
        {
            var parent = repository.GetUser(id);
            var canview = parent.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return SendJsonNoPermission();
            }

            var studentids = parent.students_guardians1.Select(x => x.studentid).ToArray();

            var viewmodel = new LateFeeAlertSiblings(studentids);

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult ShowStudent(long studentid, int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var student = repository.GetUser(studentid);
            var canview = student.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }
            var viewmodel = new SchoolFeeStudentViewModel(student, auth.perms, year.Value);

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Statement(long id, int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var parent = repository.GetUser(id);
            var canview = parent.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new StatementViewModel(parent, year.Value, auth.perms);

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS)]
        public ActionResult StudentRows(long id, long studentid)
        {
            var viewmodel = Enumerable.Empty<SchoolFeeStudent>();

            var single = db.fees.Single(x => x.id == id && x.studentid == studentid);
            if (single == null)
            {
                return SendJsonErrorResponse("Entry not found");
            }

            viewmodel = new[] { single.ToModel(auth.perms) };

            var view = this.RenderViewToString("StudentRows", viewmodel);

            return Json(view.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS)]
        public ActionResult Update(int id, FeePaymentStatus status, string amount, long studentid)
        {
            var fee = db.fees.Single(x => x.id == id && x.studentid == studentid);

            fee.amount = decimal.Parse(amount, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint);
            fee.status = status.ToString();

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var viewmodel = fee.ToModel(auth.perms);

            var view = this.RenderViewToString("StudentRows", new[] { viewmodel });

            var jsonmodel = "Fee Status updated successfully".ToJsonOKMessage();
            jsonmodel.data = view;

            return Json(jsonmodel);
        }

    }

}
