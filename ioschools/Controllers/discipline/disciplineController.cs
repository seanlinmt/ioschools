using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.sms;
using ioschools.Models;
using ioschools.Models.discipline;
using ioschools.Models.notifications;
using ioschools.Models.stats;
using ioschools.Models.user;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using ioschools.DB;

namespace ioschools.Controllers.discipline
{
    public class disciplineController : baseController
    {
        [PermissionFilter(perm = Permission.CONDUCT_CREATE)]
        public ActionResult Add()
        {
            return View();
        }

        [PermissionFilter(perm = Permission.CONDUCT_CREATE)]
        public ActionResult Delete(long id)
        {
            var d = repository.GetDiscipline(id);
            if (d == null)
            {
                return Json("Could not find entry".ToJsonFail());
            }
            if (d.creator != sessionid.Value)
            {
                return Json("You can only delete entries you've created".ToJsonFail());
            }

            d.user.updated = DateTime.Now;

            repository.DeleteDiscipline(d);

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {

                return SendJsonErrorResponse(ex);
            }


            return Json("Entry successfully deleted".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.CONDUCT_CREATE)]
        public ActionResult Edit(long? id)
        {
            var viewmodel = new Discipline();
            if (id.HasValue)
            {
                var single = repository.GetDiscipline(id.Value);
                viewmodel = single.ToModel(sessionid.Value);
                viewmodel.types = new[] { new SelectListItem() { Text = "Select type ...", Value = "" } }
                    .Union(db.conducts.OrderBy(x => x.isdemerit)
                    .ThenBy(x => x.name)
                    .Select(x => new SelectListItem()
                    {
                        Text = string.Format("{0}{1}", x.isdemerit ? "-" : "+", x.name),
                        Value = x.id.ToString(),
                        Selected = single.type.HasValue && x.id == single.type.Value
                    }));

                // get type
                if (single.conduct.max.HasValue && single.conduct.min.HasValue)
                {
                    viewmodel.isRanged = true;
                    for (int i = single.conduct.min.Value; i <= single.conduct.max.Value; i++)
                    {
                        viewmodel.ranges.Add(new SelectListItem()
                                                 {
                                                     Text = i.ToString(),
                                                     Value = i.ToString(),
                                                     Selected = i == viewmodel.points
                                                 });
                    }
                }
                else
                {
                    viewmodel.isRanged = false;
                }

            }
            else
            {
                // get user
                var usr = repository.GetUser(sessionid.Value);
                viewmodel.created = Utility.GetDBDate().ToString(Constants.DATETIME_STANDARD);
                viewmodel.creator_name = usr.ToName();
                viewmodel.types = new[] { new SelectListItem() { Text = "Select type ...", Value = "" } }
                    .Union(db.conducts.OrderBy(x => x.isdemerit)
                    .ThenBy(x => x.name)
                    .Select(x => new SelectListItem()
                                     {
                                         Text = string.Format("{0}{1}", x.isdemerit ? "-" : "+", x.name),
                                         Value = x.id.ToString()
                                     }));
            }
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult ExportStats(string from, string to, Schools school)
        {
            var start = DateTime.Parse(from);
            var end = DateTime.Parse(to);

            var stats = new ConductStatistics();

            stats.PopulateStats(start, end, school);

            var ms = new MemoryStream();
            using (var fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/NPOITemplate.xls",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var templateWorkbook = new HSSFWorkbook(fs, true);
                var sheet = templateWorkbook.CreateSheet(school.ToString());

                // create fonts
                var boldStyle = templateWorkbook.CreateCellStyle();
                var boldFont = templateWorkbook.CreateFont();
                boldFont.IsBold = true;
                boldStyle.SetFont(boldFont);

                var rowcount = 0;
                var row = sheet.CreateRow(rowcount++);

                row.CreateCell(0).SetCellValue("Behaviour Type");
                row.GetCell(0).CellStyle = boldStyle;
                row.CreateCell(1).SetCellValue("Students");
                row.GetCell(1).CellStyle = boldStyle;
                row.CreateCell(2).SetCellValue("Incidents");
                row.GetCell(2).CellStyle = boldStyle;

                foreach (var stat in stats.demerits.OrderByDescending(x => x.totalIncidents))
                {
                    row = sheet.CreateRow(rowcount++);
                    row.CreateCell(0).SetCellValue(stat.name);
                    row.CreateCell(1).SetCellValue(stat.totalStudents);
                    row.CreateCell(2).SetCellValue(stat.totalIncidents);
                }

                foreach (var stat in stats.merits.OrderByDescending(x => x.totalIncidents))
                {
                    row = sheet.CreateRow(rowcount++);
                    row.CreateCell(0).SetCellValue(stat.name);
                    row.CreateCell(1).SetCellValue(stat.totalStudents);
                    row.CreateCell(2).SetCellValue(stat.totalIncidents);
                }

                // adjust column
                sheet.AutoSizeColumn(0);

                // delete first sheet
                templateWorkbook.RemoveSheetAt(0);
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("Conduct_{0}_{1}.xls", from.Replace("/",""), to.Replace("/","")));
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.CONDUCT_CREATE)]
        public ActionResult Points(int id)
        {
            var single = db.conducts.SingleOrDefault(x => x.id == id);
            if (single == null || !single.min.HasValue || !single.max.HasValue)
            {
                return Json(new RangeJSON().ToJsonOKData());
            }
            var viewmodel = new RangeJSON()
                                {
                                    min = single.min.Value.ToString(),
                                    max = single.max.Value.ToString()
                                };

            return Json(viewmodel.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.CONDUCT_CREATE)]
        public ActionResult Save(long id, long? dip, string reason, int points, int type)
        {
            var usr = repository.GetUser(id);

            var d = new students_discipline();
            if (dip.HasValue)
            {
                d = repository.GetDiscipline(dip.Value);
            }
            d.reason = reason;
            d.type = type;

            var conduct = db.conducts.Single(x => x.id == type);

            if (!conduct.isdemerit)
            {
                // merit
                d.points = Math.Abs(points);
            }
            else
            {
                // demerit
                d.points = -Math.Abs(points);
            }
            
            if (!dip.HasValue)
            {
                d.created = DateTime.Now;
                d.creator = sessionid.Value;
                usr.students_disciplines.Add(d);
            }

            usr.updated = DateTime.Now;

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Discipline entry saved".ToJsonOKMessage());
        }

        /// <summary>
        /// dialog box for entering message to send to parent
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [PermissionFilter(perm = Permission.CONDUCT_NOTIFY)]
        public ActionResult Send(long id)
        {
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return SendJsonErrorResponse("User not found");
            }

            var viewmodel = new NotificationSendViewModel();
            viewmodel.studentid = id;
            viewmodel.message = string.Format("This is to inform you of {0}'s merit/demerit points.",
                usr.ToName());

            // get parents
            viewmodel.parents =
                usr.students_guardians.Select(
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
        [PermissionFilter(perm = Permission.CONDUCT_NOTIFY)]
        public ActionResult Send(long studentid, string message, IEnumerable<long> parent, bool use_email, bool use_SMS)
        {
            if (parent == null)
            {
                return Json("No parent/guardian was specified".ToJsonFail());
            }

            // get headmaster/headmistress emails to CC
            var year = DateTime.Now.Year;
            var schoolid =
                repository.GetUser(studentid).classes_students_allocateds.Where(x => x.year == year).Select(
                    x => x.school_class.schoolid).FirstOrDefault();
            var admins = repository.GetUsers(null, null, schoolid, null, UserGroup.HEAD, null, null, null, year, null);
            var cclist = admins.Select(x => x.email);

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
                            " School Student Conduct Info",
                            entry.email,
                            viewmodel.receiver, "", cclist);
                }

                if (use_SMS)
                {
                    Clickatell.Send(message, entry.phone_cell);

                    foreach (var number in admins.Select(x => x.phone_cell))
                    {
                        if (!string.IsNullOrEmpty(number))
                        {
                            Clickatell.Send(message, number);
                        }
                    }
                }
            }

            return Json("Notification sent successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Show(long id, int? year)
        {
            if (!year.HasValue)
            {
                year = DateTime.Now.Year;
            }

            var usr = repository.GetUser(id);

            // don't allow guardian to view other child's discipline
            var canview = usr.GetCanView(sessionid.Value,auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }
            
            var viewmodel = new DisciplineViewModel(usr, sessionid.Value, auth.group, year.Value);
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult ShowContent(long id, int year)
        {
            var usr = repository.GetUser(id);

            // don't allow guardian to view other child's discipline
            var canview = usr.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new DisciplineViewModel(usr, sessionid.Value, auth.group, year);
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult Statistics(int to_day, int to_month, int to_year, Schools school, 
            int from_day, int from_month, int from_year)
        {
            var from = new DateTime(from_year, from_month, from_day);
            var to = new DateTime(to_year, to_month, to_day);

            // get matching entries
            var viewmodel = new ConductStatistics
            {
                from = from.ToShortDateString(),
                to = to.ToShortDateString(),
                school = school.ToString()
            };
            
            viewmodel.PopulateStats(from, to, school);

            return View(viewmodel);
        }
    }
}
