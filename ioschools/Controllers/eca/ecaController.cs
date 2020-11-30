using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using ioschools.Data;
using ioschools.Data.ECA;
using ioschools.Data.User;
using ioschools.Models.user;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.eca;
using ioschools.Models.eca.stats;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace ioschools.Controllers.eca
{
    public class ecaController : baseController
    {
        [PermissionFilter(perm = Permission.ECA_CREATE)]
        public ActionResult AddStudent()
        {
            return View();
        }

        [PermissionFilter(perm = Permission.ECA_ADMIN)]
        public ActionResult Delete(long id)
        {
            try
            {
                repository.DeleteEca(id);
                HttpResponse.RemoveOutputCacheItem(Url.Action("list"));
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ECA_CREATE)]
        public ActionResult Detach(long id)
        {
            try
            {
                repository.DeleteStudentEca(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry removed successfully".ToJsonOKMessage());
        }


        [HttpGet]
        [PermissionFilter(perm = Permission.ECA_ADMIN)]
        public ActionResult Edit(long? id)
        {
            var viewmodel = new ECARow();
            if (id.HasValue)
            {
                viewmodel.eca = repository.GetEca(id.Value).ToModel();
                viewmodel.schoolList =
                    repository.GetSchools().Select(
                        x =>
                        new SelectListItem()
                            {
                                Text = x.name, 
                                Value = x.id.ToString(), 
                                Selected = (x.id == viewmodel.eca.schoolid)
                            });
            }
            else
            {
                viewmodel.schoolList =
                    repository.GetSchools().Select(
                        x => new SelectListItem()
                                 {
                                     Text = x.name, 
                                     Value = x.id.ToString()
                                 });
            }
            
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.ECA_CREATE)]
        public ActionResult EditStudent(long? id, long? studentid, int? year)
        {
            var viewmodel = new ECAStudentEditViewModel();
            if (id.HasValue)
            {
                viewmodel.eca = repository.GetStudentEca(id.Value).ToModel();
                viewmodel.schools =
                    repository.GetSchools()
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString(),
                            Selected = x.id == viewmodel.eca.schoolid
                        });
                viewmodel.ecaList = repository.GetEcas(viewmodel.eca.schoolid)
                    .OrderBy(x => x.name)
                    .Select(x => new SelectListItem()
                    {
                        Text = x.name,
                        Value = x.id.ToString(),
                        Selected = x.id == viewmodel.eca.ecaid
                    });
                viewmodel.typeList = typeof(EcaType).ToSelectList(false, "select type", "", viewmodel.eca.type);

            }
            else
            {
                if (!year.HasValue || !studentid.HasValue)
                {
                    return Json("Student not found".ToJsonFail());
                }

                var student = repository.GetUser(studentid.Value);

                var studentclass = student.classes_students_allocateds.FirstOrDefault(x => x.year == year.Value);
                if (studentclass == null)
                {
                    return Json("Please allocate a class first".ToJsonFail());
                }

                var schoolid = studentclass.school_class.schoolid;

                viewmodel.eca = new ECAStudent() { year = year.Value.ToString() };
                viewmodel.schools =
                    repository.GetSchools()
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString(),
                            Selected = x.id == schoolid
                        });
                viewmodel.ecaList = repository.GetEcas(schoolid)
                    .OrderBy(x => x.name)
                    .Select(x => new SelectListItem()
                    {
                        Text = x.name,
                        Value = x.id.ToString()
                    });
                viewmodel.typeList =
                    typeof(EcaType).ToSelectList(false, "select type", "");
            }

            var view = this.RenderViewToString("EditStudent", viewmodel);

            return Json(view.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult ExportStats(Schools school, int year)
        {
            var stats = new ECAStatistic { school = school, year = year };
            stats.CalculateStats(repository);

            var ms = new MemoryStream();
            using (var fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/NPOITemplate.xls",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
                var sheet = templateWorkbook.CreateSheet(school.ToString());

                // create fonts
                var boldStyle = templateWorkbook.CreateCellStyle();
                var boldFont = templateWorkbook.CreateFont();
                boldFont.IsBold = true;
                boldStyle.SetFont(boldFont);

                var rowcount = 0;
                var row = sheet.CreateRow(rowcount++);

                // show general stats first
                var namecell = row.CreateCell(0);
                namecell.SetCellValue("Name");
                namecell.CellStyle = boldStyle;
                var malecell = row.CreateCell(1);
                malecell.SetCellValue("Male");
                malecell.CellStyle = boldStyle;
                var femalecell = row.CreateCell(2);
                femalecell.SetCellValue("Female");
                femalecell.CellStyle = boldStyle;

                foreach (var entry in stats.entries.OrderBy(x => x.name))
                {
                    row = sheet.CreateRow(rowcount++);
                    row.CreateCell(0).SetCellValue(entry.name);
                    row.CreateCell(1).SetCellValue(entry.male);
                    row.CreateCell(2).SetCellValue(entry.female);
                }

                // resize
                sheet.AutoSizeColumn(0);
                sheet.AutoSizeColumn(1);
                sheet.AutoSizeColumn(2);

                // delete first sheet
                templateWorkbook.RemoveSheetAt(0);
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("Statistics_ECA_{0}_{1}.xls", school, year));
        }
        
        /// <summary>
        /// gets list of available ECAs by school
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [OutputCache(VaryByParam = "id", Location = OutputCacheLocation.Server, Duration = Constants.DURATION_1HOUR_SECS)]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult List(int id)
        {
            var data = repository.GetEcas(id).OrderBy(x => x.name).Select(x => new {id = x.id, name = x.name});
            return Json(data.ToJsonOKData(), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// save new or update existing eca entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="school"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.ECA_ADMIN)]
        public ActionResult Save(int? id, int school, string name)
        {
            var eca = new ioschools.DB.eca();
            if (id.HasValue)
            {
                eca = repository.GetEca(id.Value);
            }
            eca.name = name;
            eca.schoolid = school;

            if(!id.HasValue)
            {
                repository.AddEca(eca);
            }
            try
            {
                repository.Save();
                HttpResponse.RemoveOutputCacheItem(Url.Action("list", new{ id = school}));
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry saved successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.STATS_VIEW)]
        public ActionResult StatisticContent(Schools id, int year)
        {
            var stats = new ECAStatistic { school = id, year = year };
            stats.CalculateStats(repository);
            return View(stats);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult ShowStudent(long id, int year)
        {
            var student = repository.GetUser(id);

            // don't allow guardian to view other child's discipline
            var canview = student.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new ECAStudentViewModel(student, auth.perms, year);
            
            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult SaveRemark(long id, string remark)
        {
            var eca = repository.GetStudentEca(id);

            if (eca == null)
            {
                return SendJsonErrorResponse("Unable to find the student's eca");
            }

            eca.remarks = remark;
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Remarks updated successfully".ToJsonOKMessage());
        }

        /// <summary>
        /// save student eca entry
        /// </summary>
        /// <param name="id"></param>
        /// <param name="year"></param>
        /// <param name="school_eca"></param>
        /// <param name="post"></param>
        /// <param name="achievement"></param>
        /// <param name="type"></param>
        /// <param name="remarks"></param>
        /// <param name="studentid"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveStudent(long? id, int year, int school_eca, string post, string achievement, string type, 
            string remarks, long studentid)
        {
            eca_student entry;
            if (id.HasValue)
            {
                entry = repository.GetStudentEca(id.Value);
                if (entry == null)
                {
                    return Json("Unable to locate entry".ToJsonFail());
                }
            }
            else
            {
                entry = new eca_student();
                var student = repository.GetUser(studentid);
                student.eca_students.Add(entry);
                entry.year = year;
            }
            entry.ecaid = school_eca; 
            entry.type = type;
            entry.remarks = remarks;
            entry.post = post;
            entry.achievement = achievement;
            
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
    }
}
