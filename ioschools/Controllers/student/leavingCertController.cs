using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.user;
using ioschools.Models.user.student;
using NPOI.HSSF.UserModel;

namespace ioschools.Controllers.student
{
    [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE)]
    public class leavingCertController : baseController
    {
        [HttpGet]
        public ActionResult Index(long id)
        {
            var usr = repository.GetUser(id);

            // get the latest registration entry
            var viewmodel = usr.registrations.OrderByDescending(x => x.id).First().ToCertModel();

            return View(viewmodel);
        }

        [HttpPost]
        public ActionResult Export(long studentid, string reason, string remarks, string leaving)
        {
            var usr = repository.GetUser(studentid);
            var r = usr.registrations.OrderByDescending(x => x.id).First();

            var school = usr.classes_students_allocateds.OrderByDescending(x => x.year).First().school_class.schoolid;
            var date = DateTime.Now;

            // get correct template
            string templateFile = "";
            switch ((Schools)school)
            {
                case Schools.Kindergarten:
                    throw new NotImplementedException();
                    break;
                case Schools.Primary:
                    templateFile = "/Content/templates/LeavingCertPrimary.xls";
                    break;
                case Schools.Secondary:
                    templateFile = "/Content/templates/LeavingCertSecondary.xls";
                    break;
                case Schools.International:
                    templateFile = "/Content/templates/LeavingCertInternational.xls";
                    break;
            }

            // save remarks and reason
            r.cert_reason = reason;
            r.cert_remarks = remarks;
            if (!string.IsNullOrEmpty(leaving))
            {
                r.leftDate = DateTime.ParseExact(leaving, Constants.DATEFORMAT_DATEPICKER, CultureInfo.InvariantCulture);
            }
            else
            {
                r.leftDate = null;
            }
            repository.Save();

            var ms = new MemoryStream();
            using (FileStream fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + templateFile,
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);
                var sheet = templateWorkbook.GetSheetAt(0);

                // wrap style
                var wrapStyle = templateWorkbook.CreateCellStyle();
                wrapStyle.WrapText = true;

                // reference number
                var row = sheet.GetRow(6);
                row.GetCell(4).SetCellValue(usr.id);
                row.GetCell(5).SetCellValue(date.Year);

                // name
                row = sheet.GetRow(13);
                row.GetCell(3).SetCellValue(usr.ToName());

                // dob
                DateTime? dob;
                if (usr.dob.HasValue)
                {
                    dob = usr.dob.Value;
                }
                else
                {
                    dob = usr.nric_new.ToDOB();
                }
                if (dob.HasValue)
                {
                    row = sheet.GetRow(19);
                    row.GetCell(3).SetCellValue(dob.Value.ToString("dd-MMM-yyyy"));
                }

                // parents
                var guardians = usr.students_guardians;
                if (guardians.Count() != 0)
                {
                    string name = "";

                    // try get father first
                    var father = guardians.Where(x => x.type.HasValue && x.type == (byte)GuardianType.FATHER).SingleOrDefault();

                    // try get mother 
                    if (father == null)
                    {
                        var mother = guardians.Where(x => x.type.HasValue && x.type == (byte)GuardianType.MOTHER).SingleOrDefault();
                        if (mother != null)
                        {
                            name = mother.user1.name;
                        }
                    }
                    else
                    {
                        name = father.user1.name;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        row = sheet.GetRow(22);
                        row.GetCell(3).SetCellValue(name);
                    }
                }

                // admission date
                if (r.admissionDate.HasValue)
                {
                    row = sheet.GetRow(25);
                    row.GetCell(3).SetCellValue(r.admissionDate.Value.ToString("dd-MMM-yyyy"));
                }

                // leaving date
                if (r.leftDate.HasValue)
                {
                    row = sheet.GetRow(31);
                    row.GetCell(3).SetCellValue(r.leftDate.Value.ToString("dd-MMM-yyyy"));
                }

                // class
                var currentclass = usr.classes_students_allocateds.Where(x => x.year == date.Year).SingleOrDefault();
                if (currentclass != null)
                {
                    row = sheet.GetRow(28);
                    row.GetCell(3).SetCellValue(currentclass.school_class.school_year.name);
                }

                // reason
                row = sheet.GetRow(34);
                row.GetCell(3).SetCellValue(r.cert_reason);

                // remarks
                var remarkrow = sheet.GetRow(37).GetCell(3);
                remarkrow.SetCellValue(r.cert_remarks);
                remarkrow.CellStyle = wrapStyle;

                // date
                sheet.GetRow(45).GetCell(4).SetCellValue(date.ToShortDateString());

                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("LeavingCert_{0}.xls", usr.ToName()));
        }

        [HttpPost]
        public ActionResult Save(long studentid, string reason, string remarks, string leaving)
        {
            var usr = repository.GetUser(studentid);
            var r = usr.registrations.OrderByDescending(x => x.id).First();
            try
            {
                r.cert_reason = reason;
                r.cert_remarks = remarks;
                if (!string.IsNullOrEmpty(leaving))
                {
                    r.leftDate = DateTime.ParseExact(leaving, Constants.DATEFORMAT_DATEPICKER, CultureInfo.InvariantCulture);
                }
                else
                {
                    r.leftDate = null;
                }
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
