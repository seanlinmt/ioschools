using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;
using ioschools.Library.sms;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Models;
using ioschools.Models.attendance;
using ioschools.Models.email;
using ioschools.Models.notifications;
using ioschools.Models.user;

namespace ioschools.Controllers.attendance
{
    [PermissionFilter(perm = Permission.NONE)]
    public class attendanceController : baseController
    {
        [PermissionFilter(perm = Permission.ATTENDANCE_CREATE)]
        public ActionResult Add(long id)
        {
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return SendJsonErrorResponse("Unable to find user");
            }
            var viewmodel = new AttendanceAddViewModel();
            viewmodel.studentid = id; 
            viewmodel.student_name = usr.ToName();
            viewmodel.class_name = usr.ToClassName();
            viewmodel.date = Utility.GetDBDate().ToString(Constants.DATEFORMAT_DATEPICKER);
            viewmodel.dateFrom = viewmodel.dateTo = viewmodel.date;
            viewmodel.ecaList = usr.eca_students.Select(x => x.eca).Distinct().Select(x => new SelectListItem()
                                                                 {
                                                                     Text = x.name,
                                                                     Value = x.id.ToString()
                                                                 });

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.ATTENDANCE_CREATE)]
        public ActionResult Admin()
        {
            return View();
        }

        public ActionResult Content(long id, int year)
        {
            var student = repository.GetUser(id);

            var canview = student.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new AttendanceViewModel();
            viewmodel.Initialise(student, auth.group, year);

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Delete(long id)
        {
            var att = repository.GetAttendance(id);
            if (att == null)
            {
                return Json("Entry not found".ToJsonFail());
            }

            att.user.updated = DateTime.Now;

            repository.DeleteAttendance(att);

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }

            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpPost]
        public ActionResult Reason(long id, string reason)
        {
            var att = repository.GetAttendance(id);

            if (att == null)
            {
                return SendJsonErrorResponse("Unable to find attendance entry");
            }

            att.reason = reason;
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Reason updated successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ATTENDANCE_CREATE)]
        public ActionResult Save(long studentid, DateTime? date, DateTime? dateTo, DateTime? dateFrom, AttendanceDateType datetype, 
            AttendanceStatus status, AttendanceType type_group, int? eca, string reason)
        {
            // strip hours, minutes and seconds
            if (date.HasValue)
            {
                date = new DateTime(date.Value.Year, date.Value.Month, date.Value.Day);
            }
            if (dateTo.HasValue)
            {
                dateTo = new DateTime(dateTo.Value.Year, dateTo.Value.Month, dateTo.Value.Day);
            }
            if (dateFrom.HasValue)
            {
                dateFrom = new DateTime(dateFrom.Value.Year, dateFrom.Value.Month, dateFrom.Value.Day);
            }

            var usr = repository.GetUser(studentid);
            if (usr == null)
            {
                return SendJsonErrorResponse("Unable to find student");
            }

            if (!dateFrom.HasValue && !date.HasValue)
            {
                return Json("Date not specified".ToJsonFail());
            }

            var currentDate = date.HasValue ? date.Value : dateFrom.Value;

            int? classid = null;
            switch (type_group)
            {
                case AttendanceType.ECA:
                    if (!eca.HasValue)
                    {
                        return Json("ECA not selected".ToJsonFail());
                    }

                    // TODO: check that this is ECA teacher
                    // handle ranges later
                    if (datetype == AttendanceDateType.SINGLEDAY)
                    {
                        if (!date.HasValue)
                        {
                            return Json("Date not specified".ToJsonFail());
                        }
                        var at = repository.GetEcaAttendance(studentid, eca.Value, date.Value);
                        if (at == null)
                        {
                            at = new ioschools.DB.attendance()
                            {
                                date = date.Value,
                                ecaid = eca,
                                reason = reason,
                                status = status.ToString(),
                                creator = sessionid.Value
                            };
                            usr.attendances.Add(at);
                        }
                        else
                        {
                            return Json("Attendance already taken".ToJsonFail());
                        }
                    }
                    break;
                case AttendanceType.SCHOOLCLASS:
                    var year = date.HasValue ? date.Value.Year : dateFrom.Value.Year;
                    var usrclass = usr.classes_students_allocateds.SingleOrDefault(x => x.year == year);
                    if (usrclass == null)
                    {
                        return Json("Student does not have a class assigned".ToJsonFail());
                    }

                    // if teacher, make sure student is in their class
                    if (auth.group == UserGroup.TEACHER)
                    {
                        if (!repository.IsStudentInMyClass(sessionid.Value, studentid, year))
                        {
                            return Json("Student is not in your class".ToJsonFail());
                        }
                    }

                    classid = usrclass.classid;

                    // handle ranges later
                    if (datetype == AttendanceDateType.SINGLEDAY)
                    {
                        if (!date.HasValue)
                        {
                            return Json("Date not specified".ToJsonFail());
                        }
                        var att = repository.GetClassAttendance(studentid, classid.Value, date.Value);
                        if (att == null)
                        {
                            att = new ioschools.DB.attendance()
                            {
                                date = date.Value,
                                classid = classid,
                                reason = reason,
                                status = status.ToString(),
                                creator = sessionid.Value
                            };
                            usr.attendances.Add(att);
                        }
                        else
                        {
                            return Json("Attendance already taken".ToJsonFail());
                        }
                    }
                    break;
            }

            // handle date ranges
            if (datetype == AttendanceDateType.RANGE)
            {
                if (!dateFrom.HasValue || !dateTo.HasValue)
                {
                    return Json("Date not specified".ToJsonFail());
                }
                // validate dates
                if (dateTo < dateFrom)
                {
                    return Json("Invalid date range specified".ToJsonFail());
                }
                switch (type_group)
                {
                    case AttendanceType.SCHOOLCLASS:
                        if (!classid.HasValue)
                        {
                            return SendJsonErrorResponse("Unable to add attendance for the specified range");
                        }
                        while (dateFrom <= dateTo)
                        {
                            var att = repository.GetClassAttendance(studentid, classid.Value, dateFrom.Value);
                            if (att == null)
                            {
                                att = new ioschools.DB.attendance()
                                {
                                    date = dateFrom.Value,
                                    classid = classid,
                                    reason = reason,
                                    status = status.ToString(),
                                    creator = sessionid.Value
                                };
                                usr.attendances.Add(att);
                            }
                            dateFrom = dateFrom.Value.AddDays(1);
                        }
                        break;
                    case AttendanceType.ECA:
                        while (dateFrom <= dateTo)
                        {
                            var at = repository.GetEcaAttendance(studentid, eca.Value, dateFrom.Value);
                            if (at == null)
                            {
                                at = new ioschools.DB.attendance()
                                {
                                    date = dateFrom.Value,
                                    ecaid = eca,
                                    reason = reason,
                                    status = status.ToString(),
                                    creator = sessionid.Value
                                };
                                usr.attendances.Add(at);
                            }
                            dateFrom = dateFrom.Value.AddDays(1);
                        }
                        break;
                }
                
            }

            // invalidate user cache
            usr.updated = DateTime.Now;
            
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            // check if there is more than 3 entries this week
            // only include late/absent entries
            var incidents = usr.attendances
                                .Where(x => x.status != AttendanceStatus.EXCUSED.ToString())
                                .ToTotalThisWeek(currentDate);

            if (incidents >= Constants.ATTENDANCE_TRIGGER_LEVEL)
            {
                // create viewmodel
                var viewmodel = new EmailAttendanceViewModel
                                    {
                                        days = incidents,
                                        offender = usr.ToName(),
                                        link = string.Concat(Constants.HTTP_HOST, "/users/", studentid)
                                    };

                // send email to head of school
                var schoolid =
                    usr.classes_students_allocateds.Single(x => x.year == currentDate.Year).school_class.schoolid;

                var admins = repository.GetUsers(null, null, schoolid, null, UserGroup.HEAD, null, null, null, currentDate.Year, null);
                foreach (var admin in admins)
                {
                    var receiverEmail = admin.email;
                    if (!string.IsNullOrEmpty(receiverEmail))
                    {
                        viewmodel.receiver = admin.ToName();
                        this.SendEmail(
                            EmailViewType.ADMIN_ATTENDANCE,
                            viewmodel,
                            " School Attendance Info",
                            receiverEmail,
                            viewmodel.receiver);
                    }
                }
            }

            return Json("Attendance updated successfully".ToJsonOKMessage());
        }

        /// <summary>
        /// dialog box for entering message to send to parent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [PermissionFilter(perm = Permission.ATTENDANCE_NOTIFY)]
        public ActionResult Send(long id)
        {
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return Json("User not found".ToJsonFail(), JsonRequestBehavior.AllowGet);
            }

            var viewmodel = new NotificationSendViewModel();
            viewmodel.studentid = id;
            viewmodel.message = string.Format("This is to inform you that {0} has been late/absent {1} days this week.", 
                usr.ToName(),
                usr.attendances.ToNumberThisWeek(Utility.GetDBDate()).Sum());
            
            // get parents
            viewmodel.parents = usr.students_guardians.Select(
                x =>
                new IdName(x.user1.id,
                           string.Format("{0} {1}", x.user1.ToName(),
                                         string.IsNullOrEmpty(x.user1.email)
                                             ? "<span class='font_red'>no email</span>"
                                             : "")));

            return View(viewmodel);
        }

        /// <summary>
        /// sends actual message to parent
        /// </summary>
        /// <param name="studentid"></param>
        /// <param name="message"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.ATTENDANCE_NOTIFY)]
        public ActionResult Send(string message, IEnumerable<long> parent, bool use_email, bool use_SMS)
        {
            if (parent == null)
            {
                return Json("No parent/guardian was specified".ToJsonFail());
            }
            var viewmodel = new NotificationSendViewModel();
            viewmodel.message = message;

            var parentsList = new List<user>();

            foreach (var p in parent)
            {
                var usrparent = repository.GetUser(p);
                if (usrparent == null)
                {
                    return SendJsonErrorResponse("Could not locate parent");
                }

                if (use_email && string.IsNullOrEmpty(usrparent.email))
                {
                    return SendJsonErrorResponse("A parent has no email address. Notification not sent.");
                }

                if (use_SMS && string.IsNullOrEmpty(usrparent.phone_cell))
                {
                    return SendJsonErrorResponse("A parent has no cell phone number. Notification not sent.");
                }
                parentsList.Add(usrparent);
            }

            foreach (var entry in parentsList)
            {
                if (use_email)
                {
                    viewmodel.receiver = entry.ToName();
                    this.SendEmailNow(
                            EmailViewType.PARENT_NOTIFICATION,
                            viewmodel,
                            " School Attendance Info",
                            entry.email,
                            viewmodel.receiver);
                }

                if (use_SMS)
                {
                    Clickatell.Send(message, entry.phone_cell);
                }
            }

            return Json("Notification sent successfully".ToJsonOKMessage());
        }
    }

}
