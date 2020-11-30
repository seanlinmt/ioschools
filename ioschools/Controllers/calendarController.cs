using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.File;
using ioschools.Library.FileUploader;
using ioschools.Models.calendar;
using ioschools.Models.calendar.admin;
using ioschools.DB;

namespace ioschools.Controllers
{
    public class calendarController : baseController
    {
        [OutputCache(Duration = Constants.DURATION_1HOUR_SECS, VaryByParam = "None")]
        public ActionResult Index()
        {
            var date = Utility.GetDBDate();
            var calendar = new Calendar();
            var entries = calendar.GetCalendarFromDatabase().Where(x => (x.month >= date.Month && x.year >= date.Year) || x.year > date.Year);
            var serializer = new JavaScriptSerializer();

            var time = entries.Select(x => new Pair<int,int>(x.month, x.year)).Distinct().ToArray();
            var years = time.Select(x => x.Second).Distinct();

            var timeList = new Dictionary<int, IEnumerable<string>>();
            foreach (var year in years)
            {
                int year1 = year;
                timeList[year] = time.Where(x => x.Second == year1).Select(x => Calendar.months[x.First - 1]);
            }

            var viewmodel = new CalendarViewModel(baseviewmodel)
                               {
                                   calendar = serializer.Serialize(entries),
                                   months = time,
                                   time = timeList
                               };
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult Admin()
        {
            var viewmodel = new CalendarAdminViewModel(baseviewmodel);
            viewmodel.entries = repository.GetCalendarEntries().OrderByDescending(x => x.date).ToAdminModel();
            var years = repository.GetCalendarEntries()
                                .Select(x => x.date.Year)
                                .Distinct()
                                .OrderBy(x => x)
                                .Select(x => new SelectListItem()
                                {
                                    Text = x.ToString(),
                                    Value = x.ToString()
                                });
            viewmodel.yearList = new[] {new SelectListItem() {Text = "All", Value = ""}}.Union(years);

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult CalendarContent(int? id)
        {
            var viewmodel = repository.GetCalendarEntries(id).OrderByDescending(x => x.date).ToAdminModel();
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult Delete(int id)
        {
            var exist = db.calendars.SingleOrDefault(x => x.id == id);

            if (exist == null)
            {
                return Json("Entry not found".ToJsonFail());
            }

            db.calendars.DeleteOnSubmit(exist);
            db.SubmitChanges();

            return Json("Entry removed successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult Edit(int? id)
        {
            var viewmodel = new CalendarAdminEntry();
            if (id.HasValue)
            {
                var calendar = db.calendars.Single(x => x.id == id);
                viewmodel.id = id.Value.ToString();
                viewmodel.date = calendar.date;
                viewmodel.description = calendar.details;
                viewmodel.isHoliday = calendar.isHoliday;
            }
            else
            {
                viewmodel.date = Utility.GetDBDate();
            }

            return View(viewmodel);
        }

        /// <summary>
        /// this saves
        /// </summary>
        /// <param name="qqfile"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult SaveFile(string qqfile)
        {
            var uploader = new FileHandler(qqfile, UploaderType.CALENDAR, sessionid);
            bool ok = uploader.Save(Request.InputStream);

            if (!ok)
            {
                return Json("An error has occurred. Unable to save file".ToJsonFail());
            }

            // invalidate cache
            var url = Url.Action("Index", "Calendar");
            Response.RemoveOutputCacheItem(url);

            return Json("Calendar updated successfully".ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult PopulateDatabase()
        {
            var calendar = new Calendar();
            var entries = calendar.GetCalendarFromExcel();
            foreach (var entry in entries)
            {
                var date = new DateTime(entry.year, entry.month, entry.day);
                var isholiday = entry.holiday;
                foreach (var c_entry in entry.entry)
                {
                    var c = new calendar();
                    c.date = date;
                    c.isHoliday = isholiday;
                    c.details = c_entry;
                    db.calendars.InsertOnSubmit(c);
                }
            }

            db.SubmitChanges();

            return Content("");
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.CALENDAR_ADMIN)]
        public ActionResult Save(int? id, DateTime date, string details, bool holiday)
        {
            calendar entry;
            if (id.HasValue)
            {
                entry = db.calendars.Single(x => x.id == id);
            }
            else
            {
                entry = new calendar();
                db.calendars.InsertOnSubmit(entry);
            }

            entry.date = date;
            entry.details = details;
            entry.isHoliday = holiday;

            db.SubmitChanges();

            return Json("Entry updated successfully".ToJsonOKMessage());
        }
    }
}
