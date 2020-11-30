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
using ioschools.Library.email;
using ioschools.Models;
using ioschools.Models.feedback;
using ioschools.Models.user;
using clearpixels.Logging;
using ioschools.DB;

namespace ioschools.Controllers.feedback
{
    public class feedbackController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Student(long id)
        {
            var year = DateTime.Now.Year;
            var viewmodel = new StudentFeedbackViewModel();
            viewmodel.studentid = id;

            var student = repository.GetUser(id);
            if (student == null)
            {
                return SendJsonErrorResponse("Feedback: Student not found");
            }


            viewmodel.studentname = student.ToName();

            var allocated = student.classes_students_allocateds.FirstOrDefault(x => x.year == year);

            int? studentschool = null;
            IQueryable<SelectListItem> teachers = null;
            if (allocated != null)
            {
                studentschool = allocated.school_class.schoolid;

                // get teachers
                var classesAttending = student.classes_students_allocateds.Where(x => x.year == year).Select(x => x.classid).ToArray();

                var matches = repository.GetUsers().Where(x => x.usergroup == (int)UserGroup.TEACHER)
                                 .SelectMany(x => x.classes_teachers_allocateds)
                                 .Where(z => z.year == year && 
                                     classesAttending.Contains(z.classid) &&
                                     z.user.email != null && z.user.email != "");

                if (matches.Any())
                {
                    teachers = matches.Select(x => new SelectListItem()
                    {
                        Text = string.Format("{0} ({1})", x.user.ToName(true), x.subject.name),
                        Value = x.user.id.ToString()
                    });
                }
                
            }
            else
            {
                if (student.schoolid.HasValue)
                {
                    studentschool = student.schoolid.Value;
                }
            }

            // get principal
            if (studentschool.HasValue)
            {
                var matches = repository.GetUsers(null, null, (int)studentschool, null, UserGroup.HEAD, null, null, null,
                                                  year, null);
                if (matches.Any(x => x.email != "" && x.email != null))
                {
                    var principal = matches.ToArray().Select(x => new SelectListItem()
                                        {
                                            Text = x.ToName(true) + " (Principal)",
                                            Value = x.id.ToString()
                                        });

                    viewmodel.staffList.AddRange(principal);
                }
            }

            if (teachers != null)
            {
                viewmodel.staffList.AddRange(teachers);
            }

            if (viewmodel.staffList.Count == 0)
            {
                return SendJsonErrorResponse("Nobody available to contact at the moment");
            }

            return View(viewmodel);

        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Student(long id, string message, long staff)
        {
            var sender = repository.GetUser(sessionid.Value);
            var student = repository.GetUser(id);
            var contactperson = repository.GetUser(staff);

            var title = "Feedback From " + sender.ToName();

            var viewmodel = new FeedbackEmailViewModel();
            viewmodel.message = message.ToHtmlBreak();
            viewmodel.sender = new IdName(sender.id, sender.ToName());
            viewmodel.student = new IdName(student.id, student.ToName());
            viewmodel.receiver = contactperson.ToName();

            try
            {
                this.SendEmailNow(
                    EmailViewType.FEEDBACK,
                    viewmodel,
                    title,
                    contactperson.email,
                    viewmodel.receiver,
                    sender.email);
            }
            catch (Exception ex)
            {

                return SendJsonErrorResponse(ex);
            }

            return Json("Message was sent successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Vendor()
        {
            Syslog.Write(ErrorLevel.INFORMATION, "Feedback vendor clicked by " + sessionid.Value);
            return View();
        }


        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Vendor(string message)
        {
            var sender = repository.GetUser(sessionid.Value);
            var title = "Feedback From " + sender.ToName();

            var viewmodel = new FeedbackEmailViewModel();
            viewmodel.message = message.ToHtmlBreak();
            viewmodel.sender = new IdName(sender.id, sender.ToName());


            viewmodel.receiver = "Clear Pixels";
            this.SendEmailNow(
                EmailViewType.FEEDBACKVENDOR,
                viewmodel,
                title,
                "seanlinmt@clearpixels.co.nz",
                viewmodel.receiver,
                sender.email);

            return Json("Feedback submitted successfully".ToJsonOKMessage());
        }
    }
}
