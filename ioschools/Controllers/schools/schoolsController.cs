using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.Models;
using ioschools.Models.school;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.stats;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace ioschools.Controllers.schools
{
    public class schoolsController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult AddYear()
        {
            var viewmodel = new SchoolYear();
            viewmodel.gradingmethodList = new[] { new SelectListItem() { Text = "None", Value = "" } }.Union(db.grades_methods
                .OrderBy(x => x.name)
                .Select(x => new SelectListItem() { Text = x.name, Value = x.id.ToString() }));

            return View("EditYear",viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult DeleteYear(int id)
        {
            var sy = db.school_years.Single(x => x.id == id);

            try
            {
                db.school_years.DeleteOnSubmit(sy);
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return Json("Failed to delete entry".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        public ActionResult Eca()
        {
            return View();
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult EditYear(int id)
        {
            var result = repository.GetSchoolYears().Single(x => x.id == id);
            var viewmodel = result.ToModel();
            if (db.grades_methods.Any())
            {
                viewmodel.gradingmethodList = new[] { new SelectListItem() { Text = "None", Value = "" } }.Union(db.grades_methods
                .OrderBy(x => x.name)
                .Select(x => new SelectListItem()
                {
                    Text = x.name,
                    Value = x.id.ToString(),
                    Selected = x.id.ToString() == viewmodel.gradingmethodID
                }));
            }


            return View(viewmodel);
        }

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

        [OutputCache(VaryByParam = "id", Location = OutputCacheLocation.Client, Duration = Constants.DURATION_1DAY_SECS)]
        public ActionResult Classes(int? id)
        {
            if (!id.HasValue)
            {
                return new EmptyResult();
            }
            var result = repository.GetSchoolClasses().Where(x => x.schoolid == id.Value);
            var data = result.OrderBy(x => x.name).Select(x => new
            {
                x.id,
                x.name
            });
            return Json(data.ToJsonOKData(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult ExportStats(Schools school, UserGroup ugroup, int year)
        {
            var stats = new ClassStatistics { school = school, usergroup = ugroup, year = year };
            stats.CalculateStats(repository);

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
                var colcount = 0;

                // show general stats first
                row.CreateCell(colcount).SetCellValue(SecurityElement.Escape("Malaysian"));
                colcount += 2;
                row.CreateCell(colcount).SetCellValue(SecurityElement.Escape("Foreigners"));
                colcount += 2;
                row.CreateCell(colcount).SetCellValue(SecurityElement.Escape("SubTotal"));
                colcount += 2;
                row.CreateCell(colcount).SetCellValue(SecurityElement.Escape("Total"));
                row = sheet.CreateRow(rowcount++);
                for (int i = 0; i < 3; i++)
                {
                    row.CreateCell(i * 2).SetCellValue(SecurityElement.Escape("M"));
                    row.CreateCell(i * 2 + 1).SetCellValue(SecurityElement.Escape("F"));
                }
                row = sheet.CreateRow(rowcount++);
                row.CreateCell(0, CellType.Numeric).SetCellValue(stats.msian_male);
                row.CreateCell(1, CellType.Numeric).SetCellValue(stats.msian_female);
                row.CreateCell(2, CellType.Numeric).SetCellValue(stats.foreign_male);
                row.CreateCell(3, CellType.Numeric).SetCellValue(stats.foreign_female);
                row.CreateCell(4, CellType.Numeric).SetCellValue(stats.msian_male + stats.foreign_male);
                row.CreateCell(5, CellType.Numeric).SetCellValue(stats.msian_female + stats.foreign_female);
                row.CreateCell(6, CellType.Numeric).SetCellValue(stats.msian_male + stats.foreign_male + stats.msian_female + stats.foreign_female);
                
                foreach (var entry in stats.collections)
                {
                    // class row
                    row = sheet.CreateRow(rowcount++);
                    row.CreateCell(0).SetCellValue(SecurityElement.Escape(entry.name));
                    row.GetCell(0).CellStyle = boldStyle;

                    // header row1
                    row = sheet.CreateRow(rowcount++);
                    colcount = 0;
                    foreach (var race in entry.GetList())
                    {
                        row.CreateCell(colcount).SetCellValue(SecurityElement.Escape(race.name));
                        colcount += 2;
                    }
                    row.CreateCell(colcount).SetCellValue(SecurityElement.Escape("Total"));
                    
                    // header row2
                    row = sheet.CreateRow(rowcount++);
                    for (int i = 0; i < entry.GetList().Count(); i++)
                    {
                        row.CreateCell(i * 2).SetCellValue(SecurityElement.Escape("M"));
                        row.CreateCell(i * 2 + 1).SetCellValue(SecurityElement.Escape("F"));
                    }

                    // stats row
                    row = sheet.CreateRow(rowcount++);
                    colcount = 0;
                    foreach (var race in entry.GetList())
                    {
                        row.CreateCell(colcount++).SetCellValue(race.male);
                        row.CreateCell(colcount++).SetCellValue(race.female);
                    }
                    row.CreateCell(colcount).SetCellValue(entry.GetList().Sum(x => x.male + x.female));
                }
                // delete first sheet
                templateWorkbook.RemoveSheetAt(0);
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("Statistics_{0}_{1}.xls", ugroup, school));
        }
        
        [HttpPost]
        public ActionResult Subjects(int id)
        {
            var result = repository.GetSchoolSubjects(id);
            var subjects = new List<IdName>();
            foreach (var entry in result.OrderBy(x => x.name))
            {
                var subject = new IdName(entry.id, entry.name);
                subjects.Add(subject);
            }
            if (id == (int)Schools.Kindergarten)
            {
                var empty = new IdName("", "None");
                subjects.Insert(0, empty);
            }
            return Json(subjects.ToJsonOKData());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult YearRows(int? id, int? schoolid)
        {
            if (id.HasValue)
            {
                return View(new[] {db.school_years.Single(x => x.id == id.Value).ToModel()});
            }

            return View(new[] { db.school_years.Single(x => x.schoolid == schoolid.Value).ToModel() });
        }

        [HttpPost]
        public ActionResult Years(int id)
        {
            var result = repository.GetSchoolYears().Where(x => x.schoolid == id);
            var data = result.OrderBy(x => x.id).Select(x => new
            {
                x.id,
                x.name
            });
            return Json(data.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult YearsSelector()
        {
            return View();
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.SETTINGS_ADMIN)]
        public ActionResult SaveYear(int? id, string name, int? method, int schoolid)
        {
            var single = new school_year();
            if (id.HasValue)
            {
                single = db.school_years.Single(x => x.id == id);
            }
            else
            {
                db.school_years.InsertOnSubmit(single);
            }
            single.name = name;
            single.grademethodid = method;
            single.schoolid = schoolid;

            repository.Save();

            var viewmodel = "Entry saved successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("YearRows", new[]{single.ToModel()});

            return Json(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult StatisticContent(Schools id, int year)
        {
            var stats = new ClassStatistics { school = id, usergroup = UserGroup.STUDENT, year = year };
            stats.CalculateStats(repository);
            return View(stats);
        }

    }
}
