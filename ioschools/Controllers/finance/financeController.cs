using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Library.Liquid;
using ioschools.Library.email;
using ioschools.Models.fees;
using ioschools.Models.fees.liquid;
using ioschools.Models.fees.statistics;
using ioschools.Models.fees.viewmodel;
using ioschools.Models.finance;
using clearpixels.Logging;
using ioschools.DB;

namespace ioschools.Controllers.finance
{
    public class financeController : baseController
    {
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Add()
        {
            var viewmodel = new FeePayable();
            viewmodel.schoolList = repository.GetSchools().Select(x => new SelectListItem()
            {
                Text = x.name,
                Value = x.id.ToString()
            });

            return View("Edit", viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult AddFee(string name, decimal amount, long studentid)
        {
            if (string.IsNullOrEmpty(name))
            {
                return Json("Name required".ToJsonFail());
            }

            var newfee = new fee();
            newfee.name = name;
            newfee.amount = amount;
            newfee.studentid = studentid;
            newfee.status = FeePaymentStatus.UNPAID.ToString();

            // need to find due date for this entry
            try
            {
                // fee could be added after reminder has been sent or before sending a reminder
                // sent => use earliest paymentdue date after today
                var reminder_duedate = db.fees.Where(x => x.studentid == studentid &&
                                                          x.status == FeePaymentStatus.UNPAID.ToString())
                                        .SelectMany(x => x.fees_reminders)
                                        .Where(x => x.paymentdue > DateTime.Now)
                                        .OrderBy(x => x.paymentdue)
                                        .FirstOrDefault();

                if (reminder_duedate != null)
                {
                    newfee.duedate = reminder_duedate.paymentdue;
                }
                else
                {
                    // ok get fees
                    newfee.duedate = db.fees.Where(x => x.studentid == studentid &&
                                                        x.status == FeePaymentStatus.UNPAID.ToString() &&
                                                        x.duedate < DateTime.Now)
                                            .OrderByDescending(x => x.duedate)
                                            .First()
                                            .duedate;
                }

                db.fees.InsertOnSubmit(newfee);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var viewmodel = newfee.ToModel(auth.perms);

            var view = this.RenderViewToString("FeesOverdue", new[]{viewmodel});

            return Json(view.ToJsonOKData());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Alerts(long[] studentids)
        {
            var viewmodel = new LateFeeAlertSiblings(studentids);
            return View(new[] { viewmodel });
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS | Permission.FEES_ADMIN)]
        public ActionResult BulkUpdate(int schoolyear, int year, int feeid)
        {
            var feetype = db.fees_types.SingleOrDefault(x => x.id == feeid);
            if (feetype == null)
            {
                return SendJsonErrorResponse("Could not find fee");
            }

            var viewmodel = new FeeStatusUpdateViewModel();
            viewmodel.feetypeid = feeid;
            viewmodel.feename = feetype.name;
            viewmodel.year = year;
            viewmodel.schoolname = repository.GetSchoolYears().Single(x => x.id == schoolyear).school.name;

            var existingFees = db.fees.Where(x => x.typeid == feeid && 
                                                x.duedate.Year == year && 
                                                x.user.classes_students_allocateds.First(y => y.year == year).school_class.schoolyearid == schoolyear);

            if (existingFees.Any())
            {
                viewmodel.duedate = existingFees.First().duedate.ToString(Constants.DATEFORMAT_DATEPICKER_SHORT);
            }

            var existingids = existingFees.Select(x => x.studentid).ToArray();

            // get students, exclude existing fee entries
            var students = db.classes_students_allocateds
                .Where(x => x.year == year &&
                    x.school_class.school_year.id == schoolyear &&
                    !existingids.Contains(x.studentid))
                .Select(x => x.user);

            viewmodel.studentList = existingFees.ToModel(year).Union(students.ToModel(year, feetype.amount))
                .OrderBy(x => x.classname)
                .ThenBy(x => x.studentname);

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_UPDATE_STATUS | Permission.FEES_ADMIN)]
        public ActionResult BulkUpdate(int feetypeid, DateTime duedate, long?[] id, long[] studentid, string[] amount, FeePaymentStatus[] status)
        {
            var feetype = db.fees_types.SingleOrDefault(x => x.id == feetypeid);

            if (feetype == null)
            {
                return Json("Unable to find Fee Type".ToJsonFail());
            }

            for (int i = 0; i < studentid.Length; i++)
            {
                var feeid = id[i];
                var userid = studentid[i];
                var feetotal = amount[i];
                var feestatus = status[i];

                if (feeid.HasValue)
                {
                    var fee = db.fees.Single(x => x.id == feeid.Value);
                    if (feestatus == FeePaymentStatus.UNKNOWN || string.IsNullOrEmpty(feetotal))
                    {
                        db.fees.DeleteOnSubmit(fee);
                    }
                    else
                    {
                        fee.name = feetype.name;
                        fee.amount = decimal.Parse(feetotal,
                                                   NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands);
                        fee.status = feestatus.ToString();
                        fee.duedate = duedate;
                    }
                }
                else
                {
                    if (feestatus != FeePaymentStatus.UNKNOWN && !string.IsNullOrEmpty(feetotal))
                    {
                        var fee = new fee();
                        fee.name = feetype.name;
                        fee.typeid = feetypeid;
                        fee.studentid = userid;
                        fee.status = feestatus.ToString();
                        fee.amount = decimal.Parse(feetotal,
                                                   NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands);
                        fee.duedate = duedate;
                        db.fees.InsertOnSubmit(fee);
                    }
                }
            }

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }

            return Json("Fees updated successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Delete(int id)
        {
            try
            {
                var single = db.fees_types.SingleOrDefault(x => x.id == id);
                if (single == null)
                {
                    return Json("Fee Type not found".ToJsonFail());
                }
                db.fees_types.DeleteOnSubmit(single);
                repository.Save();
            }
            catch (Exception ex)
            {
                return Json("Unable to delete fee. Fee in use.".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Edit(int id)
        {
            var viewmodel = new FeePayable();
            var feetype = db.fees_types.Single(x => x.id == id);
            viewmodel.id = feetype.id.ToString();
            viewmodel.name = feetype.name;
            viewmodel.school_name = feetype.school.name;
            viewmodel.schoolList = repository.GetSchools().Select(x => new SelectListItem()
                                                                           {
                                                                               Text = x.name,
                                                                               Value = x.id.ToString(),
                                                                               Selected = x.id == feetype.schoolid
                                                                           });
            viewmodel.amount = feetype.amount.ToString("n2");

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN | Permission.FEES_UPDATE_STATUS)]
        public ActionResult Index()
        {
            var viewmodel = new FinanceViewModel(baseviewmodel);
            viewmodel.schoolList = new[]{new SelectListItem(){Value = "", Text = "Select school ..."} }.Union(
                repository.GetSchools().Select(x => new SelectListItem()
            {
                Text = x.name,
                Value = x.id.ToString()
            }));

            // don't care about reminders, otherwise if reminder is > today then it will not show up
            // check if they are any overdue fees. grouped by 
            var overdueStudents = db.fees.Where(x => x.status == FeePaymentStatus.UNPAID.ToString() &&
                                                     x.duedate < DateTime.Now)
                .OrderBy(x => x.duedate)
                .GroupBy(x => x.user);

            var alerts = new List<LateFeeAlert>();

            foreach (var entry in overdueStudents)
            {
                var rel = entry.Key.students_guardians.SingleOrDefault(x => x.type.HasValue && x.type.Value == (byte) GuardianType.FATHER);

                if (rel == null)
                {
                    rel =
                        entry.Key.students_guardians.SingleOrDefault(
                            x => x.type.HasValue && x.type.Value == (byte) GuardianType.MOTHER);
                }

                var parent = rel.user1;

                var alert = new LateFeeAlert
                                {
                                    parentid = parent.id,
                                    parentname = parent.ToName(),
                                    studentname = entry.Key.ToName(false),
                                    studentid = entry.Key.id,
                                    overdueFees = entry.Select(x => x).ToModel(auth.perms)
                                };
                alerts.Add(alert);
            }

            // group by parent
            var parentGroups = alerts.GroupBy(x => new { x.parentid, x.parentname });
            foreach (var entry in parentGroups)
            {
                var siblings = new LateFeeAlertSiblings();
                siblings.children = entry.Select(x => x);
                viewmodel.alerts.Add(siblings);
            }

            // TODO: list most overdued groups first

            // get templates
            viewmodel.templates = db.fees_templates.OrderBy(x => x.title).ToRowModel();

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN | Permission.FEES_UPDATE_STATUS)]
        public ActionResult Fees(int id)
        {
            var result = db.fees_types.Where(x => x.schoolid == id);
            var data = result.OrderBy(x => x.id).Select(x => new
            {
                x.id,
                x.name
            });
            return Json(data.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN | Permission.FEES_UPDATE_STATUS)]
        public ActionResult FeesContent(int id)
        {
            var viewmodel = db.fees_types.Where(x => x.schoolid == id).ToModel();

            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.FEES_ADMIN | Permission.FEES_UPDATE_STATUS)]
        public ActionResult ReminderHistory(int id)
        {
            var reminders = db.fees_reminders.Where(x => x.feeid == id);

            if (!reminders.Any())
            {
                return Json("No reminders have been sent yet".ToJsonFail(), JsonRequestBehavior.AllowGet);
            }
            var viewmodel = reminders.ToModel();

            var view = this.RenderViewToString("ReminderHistory", viewmodel);

            return Json(view.ToJsonOKData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        [JsonFilter(RootType = typeof(ReminderJSON), Param = "reminder")]
        public ActionResult ReminderPreview(ReminderJSON reminder)
        {
            var template = db.fees_templates.SingleOrDefault(x => x.id == reminder.templateid);

            if (template == null)
            {
                return SendJsonErrorResponse("Template not found");
            }

            var viewmodel = new Reminder();
            viewmodel.Initialise(reminder);

            var liquid = new LiquidTemplate(template.body);
            liquid.AddParameters("reminder", viewmodel);

            var view = liquid.Render();

            return Json(view.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        [JsonFilter(RootType = typeof(ReminderJSON), Param = "reminder")]
        public ActionResult ReminderSend(ReminderJSON reminder)
        {
            var template = db.fees_templates.SingleOrDefault(x => x.id == reminder.templateid);

            if (template == null)
            {
                return SendJsonErrorResponse("Template not found");
            }

            var viewmodel = new Reminder();
            viewmodel.Initialise(reminder);

            var parent = repository.GetUser(reminder.parentid);
            if (parent == null)
            {
                return SendJsonErrorResponse("Could not locate parent");
            }

            if (reminder.useEmail && string.IsNullOrEmpty(parent.email))
            {
                return SendJsonErrorResponse("Selected parent has no email address. Select another parent.");
            }

            if (reminder.useSMS && string.IsNullOrEmpty(parent.phone_cell))
            {
                return SendJsonErrorResponse("Selected parent has no cell phone number. Select another parent.");
            }

            if (reminder.useEmail)
            {
                var liquid = new LiquidTemplate(template.body);
                liquid.AddParameters("reminder", viewmodel);

                var emailbody = liquid.Render();

                new Thread(() => Email.SendMail(" School", "finance@ioschools.edu.my", parent.ToName(false), parent.email, template.title, emailbody, false, null)).Start();
            }

            // save reminder params into db
            foreach (var feeid in reminder.children.SelectMany(x => x.feeids))
            {
                var dbentry = new fees_reminder();
                dbentry.templatename = template.title;
                dbentry.created = DateTime.UtcNow;
                dbentry.paymentdue = reminder.date_due;
                dbentry.receiver = parent.id;
                dbentry.sender = sessionid.Value;
                dbentry.feeid = feeid;
                dbentry.uniqueid = viewmodel.uniqueid;
                db.fees_reminders.InsertOnSubmit(dbentry);
            }
            

            repository.Save();

            // return view
            var view = this.RenderViewToString("Alerts",
                                               new[]
                                                   {
                                                       new LateFeeAlertSiblings(
                                                           reminder.children.Select(x => x.studentid))
                                                   }).ToJsonOKData();

            view.message = "Reminder sent successfully";

            return Json(view);
        }

        

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Parent(string id)
        {
            var parent = repository.GetUser(sessionid.Value);

            // update viewed
            if (!string.IsNullOrEmpty(id))
            {
                var reminders = parent.fees_reminders.Where(x => x.uniqueid == id && !x.viewed).ToArray();

                if (reminders.Count() != 0)
                {
                    foreach (var reminder in reminders)
                    {
                        reminder.viewed = true;
                    }

                    repository.Save();
                }
            }

            var studentids = parent.students_guardians1.Select(x => x.studentid).ToArray();

            var viewmodel = new ParentFinanceViewModel(baseviewmodel)
                                {
                                    alert = new LateFeeAlertSiblings(studentids),
                                    statement = new StatementViewModel(parent, DateTime.Now.Year, auth.perms)
                                };

            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Reminder(long[] studentids)
        {
            var children =
                db.fees.Where(x => x.status == FeePaymentStatus.UNPAID.ToString() && x.duedate < DateTime.Now && studentids.Contains(x.studentid))
                .GroupBy(x => x.user);

            if (!children.Any())
            {
                return SendJsonErrorResponse("Cannot send reminder if fee is not overdue");
            }

            var viewmodel = new ReminderViewModel();

            foreach (var entry in children)
            {
                if (viewmodel.parents == null)
                {
                    viewmodel.parents = entry.Key.students_guardians.Select(x => new SelectListItem()
                                                                                     {
                                                                                         Value = x.user1.id.ToString(),
                                                                                         Text = x.user1.ToName(true)
                                                                                     });
                }

                var alert = new LateFeeAlert();
                alert.studentname = entry.Key.ToName(false);
                alert.studentid = entry.Key.id;
                alert.overdueFees = entry.Select(x => x).ToModel(auth.perms);
                viewmodel.alerts.Add(alert);
            }

            // set next due date one week from largest prev date
            var prevDate =
                viewmodel.alerts.SelectMany(x => x.overdueFees).OrderByDescending(y => y.duedate).First().duedate;

            viewmodel.nextduedate = (prevDate.HasValue? prevDate.Value: DateTime.Now).AddDays(7).ToString(Constants.DATEFORMAT_DATEPICKER);

            // init tempaltes
            viewmodel.templates = db.fees_templates.OrderBy(x => x.title).Select(x => new SelectListItem()
                                                                                          {
                                                                                              Value = x.id.ToString(),
                                                                                              Text = x.title
                                                                                          });

            var view = this.RenderViewToString("Reminder", viewmodel);

            return Json(view.ToJsonOKData(), JsonRequestBehavior.AllowGet);
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult Save(int? id, int schoolid, string name, string amount)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(amount))
            {
                return Json("Name and amount must be specified".ToJsonFail());
            }

            var feetype = new fees_type();
            if (id.HasValue)
            {
                feetype = db.fees_types.Single(x => x.id == id.Value);
            }
            feetype.name = name;
            feetype.schoolid = schoolid;
            feetype.amount = decimal.Parse(amount, NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands);

            if (!id.HasValue)
            {
                db.fees_types.InsertOnSubmit(feetype);
            }
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }

            var view = this.RenderViewToString("FeesContent", new[] { feetype.ToModel() });

            var viewmodel = "Entry saved successfully".ToJsonOKMessage();
            viewmodel.data = view;
            return Json(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult StatisticsContent(int school, int year)
        {
            var existingFees = db.fees
                .Where(x => x.duedate.Year == year && x.user.classes_students_allocateds
                            .First(y => y.year == year)
                            .school_class.schoolid == school)
                            .GroupBy(z => z.fees_type);

            var viewmodel = new StatisticsViewModel()
            {
                statname = string.Format("{0}  {1} Student Fee Statistics", year, db.schools.Single(x => x.id == school).name)
            };

            foreach (var ftype in existingFees.OrderBy(x => x.Key.name))
            {
                var byschoolyears = ftype.GroupBy(x => x.user.classes_students_allocateds.First(y => y.year == year).school_class.school_year);

                var feestat = new FeeRow()
                                  {
                                      feename = ftype.Key == null
                                              ? string.Join(", ", ftype.Select(x => x.name).ToArray())
                                              : ftype.Key.name
                                  };

                foreach (var byschoolyear in byschoolyears.OrderBy(x => x.Key.name))
                {
                    var stat = new StatRow
                    {
                        schoolyear = byschoolyear.Key.name,
                        paid = byschoolyear.Count(x => x.status == FeePaymentStatus.PAID.ToDescriptionString()),
                        unpaid = byschoolyear.Count(x => x.status == FeePaymentStatus.UNPAID.ToDescriptionString() &&
                                x.duedate > DateTime.UtcNow),
                        overdue = byschoolyear.Count(x => x.status == FeePaymentStatus.UNPAID.ToDescriptionString() &&
                                x.duedate < DateTime.UtcNow)
                    };
                    feestat.entries.Add(stat);
                }

                viewmodel.feetypes.Add(feestat);
            }

            return View(viewmodel);
        }


        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult TemplateAdd()
        {
            return View("TemplateEdit", new FeeNotificationTemplate());
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult TemplateEdit(int id)
        {
            var template = db.fees_templates.SingleOrDefault(x => x.id == id);

            if (template == null)
            {
                return Json("Template not found".ToJsonFail());
            }

            return View(template.ToModel());
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        public ActionResult TemplateRows(int id)
        {
            var viewmodel = db.fees_templates.Where(x => x.id == id).OrderBy(x => x.title).ToRowModel();

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.FEES_ADMIN)]
        [ValidateInput(false)]
        public ActionResult TemplateSave(int? id, string title, string body)
        {
            var template = new fees_template();
            if (id.HasValue)
            {
                template = db.fees_templates.SingleOrDefault(x => x.id == id);
            }
            else
            {
                db.fees_templates.InsertOnSubmit(template);
            }

            if (template == null)
            {
                return Json("Template not found".ToJsonFail());
            }

            template.title = title;
            template.body = body;

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var view = this.RenderViewToString("TemplateRows", new[] {template.ToRowModel()});
            var viewmodel = view.ToJsonOKData();
            viewmodel.message = "Template saved successfully";

            return Json(viewmodel);
        }

    }
}
