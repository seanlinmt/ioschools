using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.ECA;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.user;
using ioschools.Models.user.student;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace ioschools.Controllers.student
{
    [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE)]
    public class testimonialController : baseController
    {
        [HttpGet]
        public ActionResult Index(long id)
        {
            var usr = repository.GetUser(id);

            // get the latest registration entry
            var viewmodel = usr.registrations.OrderByDescending(x => x.id).First().ToTestimonialModel();

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Export(long studentid,
            string test_academic, string test_diligence, string test_attendance, string test_responsible, string test_initiative,
            string test_conduct, string test_honesty, string test_reliance, string test_collaborate, string test_appearance,
            string test_bm, string test_english, string test_remarks)
        {
            var usr = repository.GetUser(studentid);
            var r = usr.registrations.OrderByDescending(x => x.id).First();
            var school = usr.classes_students_allocateds.OrderByDescending(x => x.year).First().school_class.schoolid;
            var date = DateTime.Now;
            var ecas = usr.eca_students.Where(x => x.eca.schoolid == school).AsQueryable();

            // get correct template
            string templateFile = "";
            switch ((Schools)school)
            {
                case Schools.Kindergarten:
                    throw new NotImplementedException();
                    break;
                case Schools.Primary:
                    templateFile = "/Content/templates/TestimonialPrimary.xls";
                    break;
                case Schools.Secondary:
                    templateFile = "/Content/templates/TestimonialSecondary.xls";
                    break;
                case Schools.International:
                    templateFile = "/Content/templates/TestimonialInternational.xls";
                    break;
            }

            // save
            r.test_academic = test_academic;
            r.test_diligence = test_diligence;
            r.test_attendance = test_attendance;
            r.test_responsible = test_responsible;
            r.test_initiative = test_initiative;
            r.test_conduct = test_conduct;
            r.test_honesty = test_honesty;
            r.test_reliance = test_reliance;
            r.test_collaborate = test_collaborate;
            r.test_appearance = test_appearance;
            r.test_bm = test_bm;
            r.test_english = test_english;
            r.test_remarks = test_remarks;

            repository.Save();

            var ms = new MemoryStream();
            using (FileStream fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + templateFile,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
                var sheet = templateWorkbook.GetSheetAt(0);

                // small font
                var smallfont = (HSSFFont)templateWorkbook.CreateFont();
                smallfont.FontName = "Arial";
                smallfont.IsBold = false;
                smallfont.FontHeightInPoints = 8;
                var smallStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                smallStyle.SetFont(smallfont);

                // wrap style
                var wrapStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                wrapStyle.VerticalAlignment = VerticalAlignment.Top;
                wrapStyle.WrapText = true;

                // eca sytle
                var ecaStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                ecaStyle.WrapText = true;
                ecaStyle.VerticalAlignment = VerticalAlignment.Center;
                ecaStyle.Alignment = HorizontalAlignment.Center;
                ecaStyle.BorderLeft = BorderStyle.Thin;
                ecaStyle.SetFont(smallfont);

                // reference number
                var row = sheet.GetRow(5);
                row.GetCell(2).SetCellValue(usr.ToReferenceNumber(date.Year));

                // date
                row.GetCell(12).SetCellValue(date.ToShortDateString().Replace("/", "."));

                // name
                row = sheet.GetRow(8);
                row.GetCell(4).SetCellValue(usr.ToName());

                // nric
                row = sheet.GetRow(11);
                row.GetCell(3).SetCellValue(usr.nric_new);

                // admission date
                if (r.admissionDate.HasValue)
                {
                    row = sheet.GetRow(14);
                    row.GetCell(3).SetCellValue(r.admissionDate.Value.ToShortDateString().Replace("/", "."));
                }

                // leaving date
                if (r.leftDate.HasValue)
                {
                    row = sheet.GetRow(17);
                    row.GetCell(3).SetCellValue(r.leftDate.Value.ToShortDateString().Replace("/", "."));
                }

                // assessments
                sheet.GetRow(20).GetCell(5).SetCellValue(r.test_academic.ToUpper());
                sheet.GetRow(22).GetCell(5).SetCellValue(r.test_diligence.ToUpper());
                sheet.GetRow(24).GetCell(5).SetCellValue(r.test_attendance.ToUpper());
                sheet.GetRow(26).GetCell(5).SetCellValue(r.test_responsible.ToUpper());
                sheet.GetRow(28).GetCell(5).SetCellValue(r.test_initiative.ToUpper());
                sheet.GetRow(30).GetCell(5).SetCellValue(r.test_conduct.ToUpper());
                sheet.GetRow(32).GetCell(5).SetCellValue(r.test_honesty.ToUpper());
                sheet.GetRow(34).GetCell(5).SetCellValue(r.test_reliance.ToUpper());
                sheet.GetRow(36).GetCell(5).SetCellValue(r.test_collaborate.ToUpper());
                sheet.GetRow(38).GetCell(5).SetCellValue(r.test_appearance.ToUpper());
                sheet.GetRow(40).GetCell(5).SetCellValue(r.test_bm.ToUpper());
                sheet.GetRow(42).GetCell(5).SetCellValue(r.test_english.ToUpper());

                // ECA
                var sports = ecas.Where(x => x.type == EcaType.SPORTS.ToString()).OrderByDescending(x => x.year).Take(4);
                var clubs = ecas.Where(x => x.type == EcaType.CLUBS.ToString() || x.type == EcaType.UNIFORM.ToString()).OrderByDescending(x => x.year).Take(5);
                var duties = ecas.Where(x => x.type == EcaType.DUTIES.ToString()).OrderByDescending(x => x.year).Take(3);
                int rowcount = 22;

                foreach (var sport in sports)
                {
                    row = sheet.GetRow(rowcount);
                    var activitycell = row.GetCell(7);
                    activitycell.SetCellValue(sport.eca.name);
                    activitycell.CellStyle = ecaStyle;

                    var postcell = row.GetCell(11) ?? row.CreateCell(11);
                    postcell.SetCellValue(sport.post);
                    postcell.CellStyle = ecaStyle;

                    var achievementcell = row.GetCell(13) ?? row.CreateCell(13);
                    achievementcell.SetCellValue(sport.achievement);
                    achievementcell.CellStyle = ecaStyle;

                    rowcount += 2;
                }

                rowcount = 34;
                foreach (var club in clubs)
                {
                    row = sheet.GetRow(rowcount);
                    var activitycell = row.GetCell(7);
                    activitycell.SetCellValue(club.eca.name);
                    activitycell.CellStyle = ecaStyle;

                    var postcell = row.GetCell(11) ?? row.CreateCell(11);
                    postcell.SetCellValue(club.post);
                    postcell.CellStyle = ecaStyle;

                    var achievementcell = row.GetCell(13) ?? row.CreateCell(13);
                    achievementcell.SetCellValue(club.achievement);
                    achievementcell.CellStyle = ecaStyle;

                    rowcount += 2;
                }

                rowcount = 46;
                foreach (var duty in duties)
                {
                    row = sheet.GetRow(rowcount);
                    var activitycell = row.GetCell(7);
                    activitycell.SetCellValue(duty.eca.name);
                    activitycell.CellStyle = ecaStyle;

                    var postcell = row.GetCell(11) ?? row.CreateCell(11);
                    postcell.SetCellValue(duty.post);
                    postcell.CellStyle = ecaStyle;

                    var achievementcell = row.GetCell(13) ?? row.CreateCell(13);
                    achievementcell.SetCellValue(duty.year);
                    achievementcell.CellStyle = ecaStyle;

                    rowcount += 2;
                }

                // remarks
                var remarkcell = sheet.GetRow(53).GetCell(0);
                remarkcell.SetCellValue(r.test_remarks);
                remarkcell.CellStyle = wrapStyle;

                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("Testimonial_{0}.xls", usr.ToName()));
        }

        [HttpPost]
        public ActionResult Save(long studentid, 
            string test_academic, string test_diligence, string test_attendance, string test_responsible, string test_initiative,
            string test_conduct, string test_honesty, string test_reliance, string test_collaborate, string test_appearance,
            string test_bm, string test_english, string test_remarks)
        {
            var usr = repository.GetUser(studentid);
            var r = usr.registrations.OrderByDescending(x => x.id).First();
            try
            {
                // testimonial
                r.test_academic = test_academic;
                r.test_diligence = test_diligence;
                r.test_attendance = test_attendance;
                r.test_responsible = test_responsible;
                r.test_initiative = test_initiative;
                r.test_conduct = test_conduct;
                r.test_honesty = test_honesty;
                r.test_reliance = test_reliance;
                r.test_collaborate = test_collaborate;
                r.test_appearance = test_appearance;
                r.test_bm = test_bm;
                r.test_english = test_english;
                r.test_remarks = test_remarks;

                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Save successful".ToJsonOKMessage());
        }
    }
}
