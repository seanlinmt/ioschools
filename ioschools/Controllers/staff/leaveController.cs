using System;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.Leave;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Library.email;
using ioschools.Models.leave;
using ioschools.Models.notifications;
using ioschools.Models.user;
using ioschools.DB;

namespace ioschools.Controllers.staff
{
    public class leaveController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_APPLY)]
        public ActionResult Apply(long id)
        {
            var viewmodel = new LeaveTakenEdit();
            viewmodel.typeList = repository.GetUser(id).leaves_allocateds.Select(x => new SelectListItem()
                                                                                          {
                                                                                              Text = x.leave.name,
                                                                                              Value = x.leave.id.ToString()
                                                                                          });
            return View("NewLeave", viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.LEAVE_APPLY)]
        public ActionResult Index(long? id)
        {
            if (!id.HasValue)
            {
                // no id so viewing their own
                id = sessionid;
            }
            else
            {
                // has id, viewing others, so only allow to view own except admin 
                if (!auth.perms.HasFlag(Permission.LEAVE_ADMIN))
                {
                    return View("~/Views/Error/NoPermission.aspx");
                }
            }

            var staff = repository.GetUser(id.Value);

            var viewmodel = new LeaveViewModel(baseviewmodel)
                                {
                                    staffname = staff.ToName(false),
                                    staffid = id.Value,
                                    leaves = staff.leaves_allocateds.ToModel(),
                                    takenleaves = staff.leaves_takens.OrderByDescending(x => x.id).ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms),
                                    yearList = new[] {new SelectListItem() {Text = "All", Value = ""}}
                                        .Union(staff.leaves_takens
                                                   .Select(x => x.startdate.Year)
                                                   .Distinct()
                                                   .OrderBy(x => x)
                                                   .Select(x => new SelectListItem()
                                                                    {
                                                                        Text = x.ToString(),
                                                                        Value = x.ToString()
                                                                    })),
                                    canApplyLeave = id == sessionid || auth.perms.HasFlag(Permission.LEAVE_ADMIN)
                                };


            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.USERS_EDIT_STAFF)]
        public ActionResult NewStaff()
        {
            var viewmodel = new StaffLeave();
            viewmodel.typeList =
                db.leaves.OrderBy(x => x.name).Select(x => new SelectListItem() {Text = x.name, Value = x.id.ToString()});
            if (viewmodel.typeList.Any())
            {
                viewmodel.annualTotal =
                    db.leaves.Single(x => x.id.ToString() == viewmodel.typeList.First().Value).defaultTotal;
            }
            return View("EditStaff", viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_APPLY)]
        public ActionResult Cancel(long id)
        {
            var taken = db.leaves_takens.Single(x => x.id == id);
            if (taken.staffid != sessionid.Value && !auth.perms.HasFlag(Permission.LEAVE_ADMIN))
            {
                return Json("You can only cancel your own leave".ToJsonFail());
            }

            // uncomment following when approval is required from director
            //if (taken.status != (byte)LeaveStatus.APPROVED)
            {
                // just delete it
                if (taken.leaves_allocated.remaining.HasValue)
                {
                    taken.leaves_allocated.remaining += taken.days;
                }

                db.leaves_takens.DeleteOnSubmit(taken);
                repository.Save();

                return Json("Leave application deleted successfully".ToJsonOKMessage());
            }

            // add back days if leave already approved
            var alloc = taken.leaves_allocated;
            if (alloc.remaining.HasValue)
            {
                alloc.remaining += taken.days;
            }
            taken.status = (byte)LeaveStatus.CANCELLED;

            repository.Save();

            var viewmodel = "Leave application cancelled successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("IndexRows", new[] { taken }.ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms));

            return Json(viewmodel);
        }
        

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.LEAVE_APPLY)]
        public ActionResult Days(DateTime start, DateTime end, LeaveDaySegment start_time, LeaveDaySegment end_time)
        {
            var total = GetDays(start, end, start_time, end_time);

            return Json(total.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult DefaultTotal(int id)
        {
            var total = db.leaves.Single(x => x.id == id).defaultTotal;

            return Json(total.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult Delete(long id)
        {
            try
            {
                var l = db.leaves.Single(x => x.id == id);
                db.leaves.DeleteOnSubmit(l);
                repository.Save();
            }
            catch (Exception ex)
            {
                return Json("Unable to delete leave type. Leave in use.".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }


        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.USERS_EDIT_STAFF)]
        public ActionResult DeleteStaff(long id)
        {
            try
            {
                var l = db.leaves_allocateds.Single(x => x.id == id);

                db.leaves_takens.DeleteAllOnSubmit(l.leaves_takens);

                db.leaves_allocateds.DeleteOnSubmit(l);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult Edit(long? id)
        {
            var viewmodel = new AdminLeave();
            if (id.HasValue)
            {
                viewmodel = db.leaves.Single(x => x.id == id.Value).ToModel();
            }

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.USERS_EDIT_STAFF)]
        public ActionResult EditStaff(long id)
        {
            var alloc = db.leaves_allocateds.Single(x => x.id == id);
            var viewmodel = alloc.ToModel();
            viewmodel.typeList =
                db.leaves.OrderBy(x => x.name).Select(x => new SelectListItem()
                                                               {
                                                                   Text = x.name, 
                                                                   Value = x.id.ToString(),
                                                                   Selected = x.id == alloc.type
                                                               });
            return View(viewmodel);
        }


        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.LEAVE_APPLY)]
        public ActionResult IndexRows(long? id, int? year, long? staffid)
        {
            if (id.HasValue)
            {
                return View(new[] { db.leaves_takens.Single(x => x.id == id.Value) }.ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms));
            }

            var taken = db.leaves_takens.Where(x => x.staffid == staffid);
            if (year.HasValue)
            {
                taken = taken.Where(x => x.startdate.Year == year.Value);
            }

            return View(taken.ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms));
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_REVIEW)]
        public ActionResult Review(long id)
        {
            return View(id);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_REVIEW)]
        public ActionResult Review(long id, LeaveStatus status, string reason)
        {
            var taken = db.leaves_takens.Single(x => x.id == id);

            taken.status = (byte)status;
            taken.reason = reason;

            if (status == LeaveStatus.APPROVED)
            {
                // check that staff still has enough leave
                var alloc = taken.leaves_allocated;
                if (alloc.remaining.HasValue && taken.days > alloc.remaining)
                {
                    return Json("Staff does not have enough leave remaining".ToJsonFail());
                }

                if (alloc.remaining.HasValue)
                {
                    alloc.remaining -= taken.days;
                    Debug.Assert(alloc.remaining >= 0);
                }
            }

            repository.Save();

            // notify applicant
            var emailmodel = new LeaveNotification();

            emailmodel.receiver = taken.user.ToName(false);
            emailmodel.leavetakenID = taken.id;
            emailmodel.status = status.ToString();
            emailmodel.reason = reason;

            this.SendEmailNow(
                        EmailViewType.LEAVE_UPDATED,
                        emailmodel,
                        string.Format("Leave #{0} {1}", emailmodel.leavetakenID, emailmodel.status),
                        taken.user.email,
                        emailmodel.receiver);


            var viewmodel = "Leave application reviewed successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("IndexRows", new[] { taken }.ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms));

            return Json(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN)]
        public ActionResult Save(int? id, int? total, string name)
        {
            var leave = new leave();
            if (id.HasValue)
            {
                leave = db.leaves.Single(x => x.id == id.Value);
            }
            leave.name = name;
            leave.defaultTotal = total;

            if (!id.HasValue)
            {
                db.leaves.InsertOnSubmit(leave);
            }
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry saved successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.LEAVE_APPLY)]
        public ActionResult SaveIndex(long? id, int type, DateTime start, LeaveDaySegment start_time, 
            DateTime end, LeaveDaySegment end_time, long staffid, string description)
        {
            var taken = new leaves_taken();
            if (id.HasValue)
            {
                taken = db.leaves_takens.Single(x => x.id == id.Value);
            }
            else
            {
                //taken.status = (byte) LeaveStatus.PENDING;
                taken.status = (byte)LeaveStatus.APPROVED;
            }

            var alloc = db.leaves_allocateds.Single(x => x.staffid == staffid && x.type == type);

            // check that staff has enough leave remaining
            var days = GetDays(start, end, start_time, end_time);
            if (alloc.remaining.HasValue && days > alloc.remaining)
            {
                return Json("You do not have enough leave remaining".ToJsonFail());
            }

            taken.allocatedid = alloc.id;
            taken.startdate = start;
            taken.starttime = (byte) start_time;
            taken.enddate = end;
            taken.endtime = (byte) end_time;
            taken.details = description;
            taken.staffid = staffid;
            taken.days = days;

            // since we approve immediately, we have to decrement the remaining leave here
            if (alloc.remaining.HasValue)
            {
                alloc.remaining -= taken.days;
                Debug.Assert(alloc.remaining >= 0);
            }

            // save
            if (!id.HasValue)
            {
                db.leaves_takens.InsertOnSubmit(taken);
            }

            repository.Save();

            // send email notification
            /*
            var applicant = repository.GetUser(staffid);
            var emailmodel = new LeaveNotification();
            emailmodel.applicant = applicant.ToName(false);
            emailmodel.applicantid = applicant.id;

            // get director
#if DEBUG
            var director = repository.GetUsers(null, null, null, null, UserGroup.ADMIN, null, null, null, DateTime.Now.Year, null).FirstOrDefault();
            
#else
            var director = repository.GetUsers(null, null, null, null, UserGroup.DIRECTOR, null, null, null, DateTime.Now.Year, null).FirstOrDefault();
            
#endif
            if (director != null)
            {
                emailmodel.receiver = director.ToName(false);
                this.SendEmailNow(
                            EmailViewType.LEAVE_APPROVAL,
                            emailmodel,
                            string.Format("Leave Application by {0}", emailmodel.applicant),
                            director.email,
                            emailmodel.receiver);
            }
            */
            var viewmodel = "Leave application submitted successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("IndexRows", new[] { taken }.ToModel(auth.perms.HasFlag(Permission.LEAVE_REVIEW), sessionid.Value, auth.perms));

            return Json(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.USERS_EDIT_STAFF)]
        public ActionResult SaveStaff(long? id, int? total, decimal? left, int type, long staffid)
        {
            var alloc = new leaves_allocated();
            if (id.HasValue)
            {
                alloc = db.leaves_allocateds.Single(x => x.id == id.Value);
            }
            else
            {
                alloc.type = type;
                alloc.staffid = staffid;

                // check that leave type has not been allocated
                var exist = db.leaves_allocateds.Any(x => x.staffid == staffid && x.type == type);
                if (exist)
                {
                    return Json("Leave type has already been allocated to this staff.".ToJsonFail());
                }
            }

            if (total.HasValue && !left.HasValue)
            {
                return Json("You must specify leave remaining if an annual total is specified.".ToJsonFail());
            }

            if (!total.HasValue && left.HasValue)
            {
                return Json("You must specify an annual total if you want to specify remaining leave.".ToJsonFail());
            }

            alloc.annualTotal = total;
            alloc.remaining = left;

            if (!id.HasValue)
            {
                db.leaves_allocateds.InsertOnSubmit(alloc);
            }
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var viewmodel = "Entry saved successfully.".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("StaffRows", new[] {alloc}.ToModel());

            return Json(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.LEAVE_ADMIN | Permission.USERS_EDIT_STAFF)]
        public ActionResult SingleStaff(long id)
        {
            var alloc = db.leaves_allocateds.Single(x => x.id == id);

            var viewmodel = new[] {alloc}.ToModel();

            return View("StaffRows", viewmodel);
        }

        private decimal GetDays(DateTime start, DateTime end, LeaveDaySegment starttime, LeaveDaySegment endtime)
        {
            decimal total = (end - start).Days + 1;

            if (total != 0)
            {
                if (starttime == LeaveDaySegment.START && endtime == LeaveDaySegment.MIDDLE)
                {
                    total -= (decimal)0.5;
                }
                else if (starttime == LeaveDaySegment.START && endtime == LeaveDaySegment.END)
                {
                    // as usual
                }
                else if (starttime == LeaveDaySegment.MIDDLE && endtime == LeaveDaySegment.END)
                {
                    total -= (decimal)0.5;
                }
                else if (starttime == LeaveDaySegment.MIDDLE && endtime == LeaveDaySegment.MIDDLE)
                {
                    total -= 1;
                }
            }

            return total;
        }
    }
}
