using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;
using ioschools.Areas.exams.Models;
using ioschools.Areas.exams.Models.remarks;
using ioschools.Controllers;
using ioschools.Library.Extensions;
using ioschools.Models.attendance;
using ioschools.Models.exam.statistics;
using ioschools.Models.exam.viewmodels;
using ioschools.Models.subject;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.exam;
using ioschools.Models.exam.JSON;
using ioschools.Models.exam.templates;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;

namespace ioschools.Areas.exams.Controllers
{
    public class examsController : baseController
    {
        [NoCache]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE)]
        public ActionResult Add()
        {
            return View(baseviewmodel);
        }

        
        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE)]
        public ActionResult Delete(long id)
        {
            // only allow creator to delete exam
            var exam = repository.GetExam(id);
            if (exam == null)
            {
                return Json("Exam not found".ToJsonFail());
            }

            if (!auth.perms.HasFlag(Permission.EXAM_ADMIN) && 
                exam.creator != sessionid.Value)
            {
                return Json("Only exam creator can delete this entry".ToJsonFail());
            }

            try
            {
                // delete any exams or subjects but will fail if there are already marks assigned
                var classes = db.exam_classes.Where(x => x.examid == id);
                db.exam_classes.DeleteAllOnSubmit(classes);

                var subjects = db.exam_subjects.Where(x => x.examid == id);
                db.exam_subjects.DeleteAllOnSubmit(subjects);

                var marks = db.exam_marks.Where(x => x.examid == id);
                db.exam_marks.DeleteAllOnSubmit(marks);

                db.exams.DeleteOnSubmit(exam);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Exam delete successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult ExamContent(long id, int year)
        {
            var canview = false;
            var usr = repository.GetUser(id);
            if (sessionid.HasValue)
            {
                canview = usr.GetCanView(sessionid.Value, auth);
            }

            if (!canview)
            {
                return new EmptyResult();
            }
            // don't allow other parents or students to view this student's result
            var viewmodel = usr.exam_marks.Where(x => x.exam.exam_date.Year == year).ToModel();
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_VIEW | Permission.EXAM_EDIT)]
        public ActionResult Export(int? year, int? school, int? sclass, long? id)
        {
            IEnumerable<exam> exams;
            if (id.HasValue)
            {
                var exam = repository.GetExam(id.Value);
                exams = new[] {exam};
            }
            else
            {
                exams = repository.GetExams(school, sclass, year);
            }
            

            var ms = new MemoryStream();
            using (var fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/NPOITemplate.xls",
                    FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var templateWorkbook = new HSSFWorkbook(fs, true);

                foreach (var entry in exams)
                {
                    var exam = entry.ToMarksModel();
                    var sheetname = exam.id + " " + exam.name;
                    var sheet = templateWorkbook.CreateSheet(sheetname.ToSafeFilename());
                    
                    var rowcount = 0;
                    var row = sheet.CreateRow(rowcount);

                    var colcount = 1;
                    foreach (var subject in exam.subjects)
                    {
                        row.CreateCell(colcount++).SetCellValue(SecurityElement.Escape(subject.name));
                    }
                    rowcount++;
                    foreach (var section in exam.sections)
                    {
                        // if class specified then only include specified classes
                        if (sclass.HasValue && sclass.Value != section.class_id)
                        {
                            continue;
                        }
                        row = sheet.CreateRow(rowcount++);
                        row.CreateCell(0).SetCellValue(SecurityElement.Escape(section.class_name));
                        foreach (var smark in section.marks)
                        {
                            row = sheet.CreateRow(rowcount++);
                            row.CreateCell(0).SetCellValue(SecurityElement.Escape(smark.student.name));
                            var markcol = 1;
                            foreach (var mark_entry in smark.marks)
                            {
                                int mark;
                                if (int.TryParse(mark_entry.name, out mark))
                                {
                                    row.CreateCell(markcol++).SetCellValue(mark);
                                }
                                else
                                {
                                    row.CreateCell(markcol++).SetCellValue(mark_entry.name);
                                }
                                
                            }
                        }
                    }
                    sheet.AutoSizeColumn(0);
                }
                // delete first sheet
                templateWorkbook.RemoveSheetAt(0);
                templateWorkbook.Write(ms);
            }

            // return created file path);
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("ExamResults_{0}.xls", DateTime.Now.ToShortDateString().Replace("/","")));

        }

        [NoCache]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_VIEW | Permission.EXAM_EDIT)]
        public ActionResult Index()
        {
            var viewmodel = new ExamDashboardViewModel(baseviewmodel);

            viewmodel.yearlist = db.exams
                .Select(x => x.exam_date.Year)
                .Distinct()
                .OrderByDescending(x => x)
                .Select(
                    x => new SelectListItem() {Text = x.ToString(), Value = x.ToString()});

            viewmodel.schools =
                new[] {new SelectListItem() {Text = "All Schools", Value = ""}}.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                                         {
                                             Text = x.name,
                                             Value = x.id.ToString()
                                         }));

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_VIEW | Permission.EXAM_EDIT)]
        public ActionResult List(int? school, int? form, int rows, int page, int? year)
        {
            IQueryable<exam> results = repository.GetExams(school, form, year);

            // return in the format required for jqgrid
            results = results.OrderByDescending(x => x.id);
            var exams = results.Skip(rows * (page - 1)).Take(rows).ToExamsJqGrid();
            var records = results.Count();
            var total = (records / rows);
            if (records % rows != 0)
            {
                total++;
            }

            exams.page = page;
            exams.records = records;
            exams.total = total;

            return Json(exams);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Remarks(long id)
        {
            var student = repository.GetUser(id);

            var canview = student.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }
            var viewmodel = new StudentRemarksViewModel(student, auth.perms, DateTime.Now.Year);
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult RemarksContent(long id, int year)
        {
            var student = repository.GetUser(id);

            var canview = student.GetCanView(sessionid.Value, auth);
            if (!canview)
            {
                return ReturnNoPermissionView();
            }
            var viewmodel = new ExamRemarksViewModel(auth.perms);
            var firstclass = student.classes_students_allocateds.FirstOrDefault(x => x.year == year);
            if (firstclass != null)
            {
                var schoolterms = firstclass.school_class.school.school_terms;
                foreach (var entry in schoolterms)
                {
                    var term = entry;
                    var dbremark =
                        student.students_remarks.SingleOrDefault(x => x.year == year && x.term == term.id);
                    var remark = new StudentRemark();
                    remark.term_id = entry.id;
                    if (dbremark != null)
                    {
                        remark = dbremark.ToModel();
                        viewmodel.remarks.Add(remark);
                    }
                    else if (viewmodel.canEdit)
                    {
                        remark.term_name = term.name;
                        remark.year = year;
                        viewmodel.remarks.Add(remark);
                    }
                    else
                    {
                        // don't add empty entry 
                    }
                    
                }
            }
            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE)]
        public ActionResult ReportCard(long id)
        {
            var usr = repository.GetUser(id);

            // get exams users are in
            var viewmodel = new ReportCardExamSelectViewModel();

            // list exam years
            var years = usr.exam_marks.Select(x => x.exam.year).Distinct().OrderByDescending(x => x);

            if (!years.Any())
            {
                return Json("No results found".ToJsonFail(), JsonRequestBehavior.AllowGet);
            }

            viewmodel.yearList = years
                .Select(y => new SelectListItem() {Text = y.ToString(), Value = y.ToString()})
                .ToList();

            var year = years.FirstOrDefault();

            // list exam terms
            var schoolid = usr.classes_students_allocateds
                .Where(x => x.year == year)
                .Select(x => x.school_class.schoolid)
                .FirstOrDefault();

            var terms = repository.GetSchoolTerms().Where(x => x.schoolid == schoolid);
            viewmodel.termList = terms
                .Select(x => new SelectListItem(){Text = x.name, Value = x.id.ToString()})
                .ToList();

            // list exams
            viewmodel.examList =
                usr.exam_marks.Select(x => x.exam)
                    .Distinct()
                    .Select(x => new SelectListItem() {Text = x.name, Value = x.id.ToString()});

            viewmodel.student_name = usr.ToName();
            viewmodel.studentid = usr.id;

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE)]
        public ActionResult ReportCard(long studentid, short? term, int year, bool allclass, long? result)
        {
            var usr = repository.GetUser(studentid);

            // get class
            var student_class = usr.classes_students_allocateds.SingleOrDefault(x => x.year == year);
            var schoolyear = student_class.school_class.school_year.name;

            string template_path = "";
            var _school = (Schools) student_class.school_class.schoolid;
            var minSubjectRows = 0;
            switch (_school)
            {
                case Schools.Kindergarten:
                    template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardKindergarten.xls";
                    minSubjectRows = 7;
                    break;
                case Schools.Primary:
                    if (schoolyear == "Tahun 1" || schoolyear == "Tahun 2")
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardKSSR.xls";
                        minSubjectRows = 10;
                    }
                    else
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardKBSR.xls";
                        minSubjectRows = 9;
                    }
                    break;
                case Schools.Secondary:
                    if (schoolyear == "Tingkatan 1" ||
                        schoolyear == "Tingkatan 2" ||
                        schoolyear == "Tingkatan 3")
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardKBSMRendah.xls";
                        minSubjectRows = 9;
                    }
                    else
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardKBSMAtas.xls";
                        minSubjectRows = 14;
                    }
                    break;
                case Schools.International:
                    if (term.HasValue && term.Value == 11 &&
                        (student_class.school_class.name == "Year 11A" ||
                         student_class.school_class.name == "Year 11B" ||
                         student_class.school_class.name == "Year 12A" ||
                         student_class.school_class.name == "Year 12B"
                        ))
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardInternationalExtraSubHeader.xls";
                    }
                    else
                    {
                        template_path = AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardInternational.xls";
                    }
                    minSubjectRows = 10;
                    break;
            }

            var students = new List<user>();
            if (allclass)
            {
                var usrs = student_class.school_class.classes_students_allocateds
                    .Where(x => x.year == year && (x.user.settings & (int) UserSettings.INACTIVE) == 0)
                    .Select(x => x.user);
                students.AddRange(usrs);
            }
            else
            {
                students.Add(usr);
            }

            var ms = new MemoryStream();
            using (var fs = new FileStream(template_path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
#if !DEBUG
                try
#endif
                {
                    var templateWorkbook = new HSSFWorkbook(fs, true);
                    var middleAlignStyle = templateWorkbook.CreateCellStyle();
                    middleAlignStyle.Alignment = HorizontalAlignment.Center;

                    var middleAlignBorderRightStyle = templateWorkbook.CreateCellStyle();
                    middleAlignBorderRightStyle.Alignment = HorizontalAlignment.Center;
                    middleAlignBorderRightStyle.RightBorderColor = IndexedColors.Black.Index;
                    middleAlignBorderRightStyle.BorderRight = BorderStyle.Thin;
                    middleAlignBorderRightStyle.VerticalAlignment = VerticalAlignment.Center;
                    
                    var middleAlignBorderLeftStyle = templateWorkbook.CreateCellStyle();
                    middleAlignBorderLeftStyle.Alignment = HorizontalAlignment.Center;
                    middleAlignBorderLeftStyle.LeftBorderColor = IndexedColors.Black.Index;
                    middleAlignBorderLeftStyle.BorderLeft = BorderStyle.Thin;
                    middleAlignBorderLeftStyle.VerticalAlignment = VerticalAlignment.Center;

                    var middleAlignBorderLeftRightStyle = templateWorkbook.CreateCellStyle();
                    middleAlignBorderLeftRightStyle.Alignment = HorizontalAlignment.Center;
                    middleAlignBorderLeftRightStyle.RightBorderColor = IndexedColors.Black.Index;
                    middleAlignBorderLeftRightStyle.BorderRight = BorderStyle.Thin;
                    middleAlignBorderLeftRightStyle.LeftBorderColor = IndexedColors.Black.Index;
                    middleAlignBorderLeftRightStyle.BorderLeft = BorderStyle.Thin;

                    var nameStyle = templateWorkbook.CreateCellStyle();
                    nameStyle.VerticalAlignment = VerticalAlignment.Center;
                    nameStyle.Alignment = HorizontalAlignment.Center;
                    nameStyle.BorderTop = BorderStyle.Thin;
                    nameStyle.BorderLeft = BorderStyle.Thin;

                    var smallfont = templateWorkbook.CreateFont();
                    smallfont.FontName = "Arial";
                    smallfont.IsBold = false;
                    smallfont.FontHeightInPoints = 8;

                    var boldfont = templateWorkbook.CreateFont();
                    boldfont.FontName = "Arial";
                    boldfont.IsBold = true;
                    boldfont.FontHeightInPoints = 8;

                    var smallStyle = templateWorkbook.CreateCellStyle();
                    smallStyle.SetFont(smallfont);

                    var smallBorderLeftStyle = templateWorkbook.CreateCellStyle();
                    smallBorderLeftStyle.SetFont(smallfont);
                    smallBorderLeftStyle.BorderLeft = BorderStyle.Thin;
                    smallBorderLeftStyle.LeftBorderColor = IndexedColors.Black.Index;

                    // italic font
                    var italicFont = templateWorkbook.CreateFont();
                    italicFont.IsItalic = true;
                    italicFont.FontHeightInPoints = 8;

                    // wrap style
                    var smallWrapStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    smallWrapStyle.WrapText = true;
                    smallWrapStyle.VerticalAlignment = VerticalAlignment.Top;
                    smallWrapStyle.SetFont(smallfont);

                    var smallWrapRightBorderStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    smallWrapRightBorderStyle.WrapText = true;
                    smallWrapRightBorderStyle.VerticalAlignment = VerticalAlignment.Top;
                    smallWrapRightBorderStyle.SetFont(smallfont);
                    smallWrapRightBorderStyle.BorderRight = BorderStyle.Thin;
                    smallWrapRightBorderStyle.RightBorderColor = IndexedColors.Black.Index;

                    // center align style
                    var alignCenterStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    alignCenterStyle.Alignment = HorizontalAlignment.Center;

                    var alignCenterTopBorderStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    alignCenterTopBorderStyle.Alignment = HorizontalAlignment.Center;
                    alignCenterTopBorderStyle.BorderTop = BorderStyle.Thin;
                    alignCenterTopBorderStyle.TopBorderColor = IndexedColors.Black.Index;
                    
                    // right align style
                    var alignRightStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    alignRightStyle.Alignment = HorizontalAlignment.Right;
                    alignRightStyle.SetFont(smallfont);

                    var alignRightTopBorderStyle = (HSSFCellStyle)templateWorkbook.CreateCellStyle();
                    alignRightTopBorderStyle.Alignment = HorizontalAlignment.Right;
                    alignRightTopBorderStyle.BorderTop = BorderStyle.Thin;
                    alignRightTopBorderStyle.TopBorderColor = IndexedColors.Black.Index;

                    var picture = (HSSFPictureData)templateWorkbook.GetAllPictures()[0];

                    var userindex = 1;
                    foreach (var student in students)
                    {
                        var sheet = templateWorkbook.CloneSheet(0);
                        templateWorkbook.SetSheetName(userindex++, student.name);
                        var pictidx = templateWorkbook.AddPicture(picture.Data, (PictureType)picture.Format);
                        var patriarch = sheet.CreateDrawingPatriarch();
                        var anchor = new HSSFClientAnchor(0, 0, 0, 0, 0, 0, 0, 3);
                        anchor.AnchorType = AnchorType.DontMoveAndResize;
                        var logo = patriarch.CreatePicture(anchor, pictidx);
                        logo.Resize(0.85);

                        exam exam = null;
                        if (result.HasValue)
                        {
                            exam = student.exam_marks.Where(x => x.examid == result)
                                        .Select(x => x.exam).Distinct().SingleOrDefault();
                        }

                        // starting row
                        var rowcount = 7;
                        if (term.HasValue && term.Value == 11 &&
                                (student_class.school_class.name == "Year 11A" ||
                                 student_class.school_class.name == "Year 11B" ||
                                 student_class.school_class.name == "Year 12A" ||
                                 student_class.school_class.name == "Year 12B"
                                ))
                        {
                            if (student_class.school_class.name == "Year 11A" ||
                                student_class.school_class.name == "Year 11B")
                            {
                                sheet.GetRow(rowcount).GetCell(0).SetCellValue("IGCSE Trial Examination Results");
                            }
                            else if (student_class.school_class.name == "Year 12A" ||
                                student_class.school_class.name == "Year 12B")
                            {
                                sheet.GetRow(rowcount).GetCell(0).SetCellValue("AS/A Levels Trial Examination Results");
                            }
                            rowcount += 2;
                        }
                        else
                        {
                            rowcount++;
                        }
                        

                        // student details
                        var row = sheet.GetRow(rowcount);

                        // student name
                        var namecell = row.GetCell(1);
                        namecell.SetCellValue(student.name);
                        namecell.CellStyle = nameStyle;

                        // student class
                        var classcell = row.GetCell(5);
                        classcell.SetCellValue(student_class.school_class.name);

                        // year
                        var yearcell = row.GetCell(7);
                        yearcell.SetCellValue(year);

                        // dob
                        if (student.dob.HasValue)
                        {
                            var dobcell = row.GetCell(10);
                            dobcell.SetCellValue(student.dob.Value.ToShortDateString().Replace("/", "."));
                        }

                        // term
                        if (term.HasValue)
                        {
                            var termstring = repository.GetSchoolTerms().Where(x => x.id == term.Value).Single().name;
                            row = sheet.GetRow(rowcount + 3);
                            var termcell = row.GetCell(8);

                            var insertionIndex = termstring.IndexOf("(");
                            if (insertionIndex != -1)
                            {
                                var str = termstring.Insert(insertionIndex, Environment.NewLine).Remove(insertionIndex - 1, 2);
                                var formattedTermString = new HSSFRichTextString(str);
                                formattedTermString.ApplyFont(smallfont);
                                formattedTermString.ApplyFont(0, insertionIndex, boldfont);
                                termcell.SetCellValue(formattedTermString);
                            }
                            else
                            {
                                var str = new HSSFRichTextString(termstring);
                                str.ApplyFont(boldfont);
                                termcell.SetCellValue(str);    
                            }
                        }

                        var subjectcount = 0;
                        var shiftedrows = rowcount + 5;
                        rowcount = shiftedrows;
                        if (exam != null)
                        {
                            // gpa 0 is ignored. bug?? but possibly unlikely to occur
                            var totalMarks = 0;
                            var totalTotallableMarks = 0;
                            decimal gpa1 = 0;
                            
                            foreach (var subject in exam.exam_subjects.OrderBy(x => x.position))
                            {
                                row = sheet.GetRow(rowcount);
                                var subjectid = subject.id;
                                var subjectname = subject.name;
                                var hasMark = false; // covers has mark and absent

                                //  marks
                                var mark1 = exam.exam_marks.SingleOrDefault(x => x.exam_subjectid == subjectid && x.studentid == student.id);
                                if (mark1 != null && (!string.IsNullOrEmpty(mark1.mark) || (mark1.absent)))
                                {
                                    hasMark = true;
                                    var markcell = row.CreateCellIfNotExist(8);
                                    markcell.CellStyle = middleAlignBorderLeftStyle;
                                    var gradecell = row.CreateCellIfNotExist(9);
                                    gradecell.CellStyle = middleAlignBorderRightStyle;
                                    if (!string.IsNullOrEmpty(mark1.mark))
                                    {
                                        short markvalue;
                                        if (short.TryParse(mark1.mark, out markvalue) && !mark1.absent)
                                        {
                                            totalMarks += markvalue;
                                            totalTotallableMarks++;
                                            var grade = markvalue.ToGrade(student_class.school_class.school_year);
                                            if (grade.gradepoint.HasValue)
                                            {
                                                gpa1 += grade.gradepoint.Value;
                                            }

                                            // set grade
                                            gradecell.SetCellValue(markvalue.ToGrade(student_class.school_class.school_year).grade);
                                            
                                            // set mark
                                            markcell.SetCellValue(markvalue);
                                        }
                                        else
                                        {
                                            // set mark type grade
                                            if (mark1.absent)
                                            {
                                                // set absent
                                                markcell.SetCellValue("X");
                                            }
                                            else
                                            {
                                                if (_school == Schools.Kindergarten)
                                                {
                                                    // set mark of grade type
                                                    markcell.SetCellValue(mark1.mark);
                                                }
                                                else
                                                {
                                                    // set mark of grade type
                                                    gradecell.SetCellValue(mark1.mark);
                                                }
                                                
                                            }
                                        }
                                    }
                                    else if (mark1.absent)
                                    {
                                        // set absent
                                        markcell.SetCellValue("X");
                                    }

                                    if (_school == Schools.Kindergarten)
                                    {
                                        row.GetCell(8).CellStyle = middleAlignBorderLeftStyle;
                                        row.GetCell(9).CellStyle = middleAlignBorderRightStyle;
                                        sheet.AddMergedRegion(new CellRangeAddress(rowcount, rowcount, 8, 9));
                                    }
                                }

                                if (hasMark)
                                {
                                    // set code
                                    sheet.AddMergedRegion(new CellRangeAddress(rowcount, rowcount, 1, 7));
                                    var codecell = row.CreateCellIfNotExist(0);
                                    codecell.SetCellValue(subject.code);
                                    codecell.CellStyle = smallBorderLeftStyle;

                                    // set subject
                                    var subjectcell = row.CreateCellIfNotExist(1);
                                    subjectcell.SetCellValue(subjectname);
                                    subjectcell.CellStyle = smallWrapRightBorderStyle;

                                    // need to adjust height for long subject names
                                    if (subjectname.Length > 72)
                                    {
                                        row.HeightInPoints =  24;
                                    }

                                    subjectcount++;
                                    rowcount++;
                                }
                            }

                            shiftedrows += subjectcount;

                            // create rows for total marks
                            if (_school != Schools.Kindergarten)  // if not kindy
                            {
                                var totalrow = sheet.GetRow(rowcount) ?? sheet.CreateRow(rowcount);
                                rowcount++;
                                var averagerow = sheet.GetRow(rowcount) ?? sheet.CreateRow(rowcount);
                                rowcount++;
                                var gparow = sheet.GetRow(rowcount) ?? sheet.CreateRow(rowcount);
                                rowcount++;
                                shiftedrows += 3;

                                for (int i = 0; i < 8; i++)
                                {
                                    totalrow.GetCell(i).CellStyle = alignRightTopBorderStyle;
                                }

                                if (_school == Schools.International)
                                {
                                    var totalString = new HSSFRichTextString("Total Mark");
                                    totalString.ApplyFont(boldfont);
                                    sheet.AddMergedRegion(new CellRangeAddress(totalrow.RowNum, totalrow.RowNum, 0, 7));
                                    totalrow.CreateCellIfNotExist(0).SetCellValue(totalString);

                                    var averageString = new HSSFRichTextString("Average Mark");
                                    averageString.ApplyFont(boldfont);
                                    sheet.AddMergedRegion(new CellRangeAddress(averagerow.RowNum, averagerow.RowNum, 0, 7));
                                    averagerow.CreateCellIfNotExist(0).SetCellValue(averageString);
                                    averagerow.CreateCellIfNotExist(0).CellStyle = alignRightStyle;
                                }
                                else
                                {
                                    var totalString = new HSSFRichTextString("Jumlah Markah (Total Mark)");
                                    totalString.ApplyFont(0, totalString.String.IndexOf("(") - 1, boldfont);
                                    totalString.ApplyFont(totalString.String.IndexOf("("), totalString.String.Length, italicFont);
                                    sheet.AddMergedRegion(new CellRangeAddress(totalrow.RowNum, totalrow.RowNum, 0, 7));
                                    totalrow.CreateCellIfNotExist(0).SetCellValue(totalString);

                                    var averageString = new HSSFRichTextString("Purata Markah (Average Mark)");
                                    averageString.ApplyFont(0, averageString.String.IndexOf("(") - 1, boldfont);
                                    averageString.ApplyFont(averageString.String.IndexOf("("), averageString.String.Length, italicFont);
                                    sheet.AddMergedRegion(new CellRangeAddress(averagerow.RowNum, averagerow.RowNum, 0, 7));
                                    averagerow.CreateCellIfNotExist(0).SetCellValue(averageString);
                                    averagerow.CreateCellIfNotExist(0).CellStyle = alignRightStyle;

                                    var gpaString = new HSSFRichTextString("Gred Purata Murid (Student's Average Grade)");
                                    gpaString.ApplyFont(0, gpaString.String.IndexOf("(") - 1, boldfont);
                                    gpaString.ApplyFont(gpaString.String.IndexOf("("), gpaString.String.Length, italicFont);
                                    sheet.AddMergedRegion(new CellRangeAddress(gparow.RowNum, gparow.RowNum, 0, 7));
                                    gparow.CreateCellIfNotExist(0).SetCellValue(gpaString);
                                    gparow.CreateCellIfNotExist(0).CellStyle = alignRightStyle;
                                }
                                
                                var total1Cell = totalrow.CreateCellIfNotExist(8);
                                totalrow.GetCell(8).CellStyle = alignCenterTopBorderStyle;
                                totalrow.GetCell(9).CellStyle = alignCenterTopBorderStyle;
                                sheet.AddMergedRegion(new CellRangeAddress(totalrow.RowNum, totalrow.RowNum, 8, 9));
                                total1Cell.SetCellValue(totalMarks);

                                var average1Cell = averagerow.CreateCellIfNotExist(8);
                                sheet.AddMergedRegion(new CellRangeAddress(averagerow.RowNum, averagerow.RowNum, 8, 9));
                                average1Cell.SetCellValue(((double)totalMarks / totalTotallableMarks).ToString("n2"));
                                average1Cell.CellStyle = alignCenterStyle;

                                var gpa1Cell = gparow.CreateCellIfNotExist(8);
                                sheet.AddMergedRegion(new CellRangeAddress(gparow.RowNum, gparow.RowNum, 8, 9));
                                if (gpa1 != 0)
                                {
                                    gpa1Cell.SetCellValue(((double)gpa1 / totalTotallableMarks).ToString("n2"));
                                    gpa1Cell.CellStyle = alignCenterStyle;
                                }
                            }
                            else
                            {
                                var aftersubjectrow = sheet.GetRow(rowcount) ?? sheet.CreateRow(rowcount);
                                for (int i = 0; i < 10; i++)
                                {
                                    aftersubjectrow.CreateCellIfNotExist(i).CellStyle = alignCenterTopBorderStyle;
                                }
                                rowcount++;
                                shiftedrows++;
                            }
                        }

                        // need to add additional lines so that grade key not overwritten
                        if (subjectcount < minSubjectRows)
                        {
                            shiftedrows += (minSubjectRows - subjectcount);
                            rowcount += (minSubjectRows - subjectcount);
                        }

                        ////////////////////// handle eca
                        var ecacount = 0;
                        if (_school != Schools.Kindergarten)  // excludes kindy
                        {
                            sheet.CreateRow(rowcount++);
                            shiftedrows++; // blank line
                            sheet.CreateRow(rowcount++);
                            sheet.CreateRow(rowcount++);
                            sheet.ShiftRows(41, 42, 0 - (41 - shiftedrows));
                            shiftedrows += 2;

                            foreach (var eca in student.eca_students.Where(x => x.year == year))
                            {
                                var maxHeightMultiple = 0;
                                sheet.AddMergedRegion(new CellRangeAddress(rowcount, rowcount, 0, 2));
                                sheet.AddMergedRegion(new CellRangeAddress(rowcount, rowcount, 3, 5));
                                sheet.AddMergedRegion(new CellRangeAddress(rowcount, rowcount, 6, 11));
                                row = sheet.CreateRow(rowcount++);
                                var activitycell = row.CreateCellIfNotExist(0);
                                var postcell = row.CreateCellIfNotExist(3);
                                var achievementcell = row.CreateCellIfNotExist(6);
                                activitycell.SetCellValue(eca.eca.name);
                                var heightMultiple = eca.post.Length / 25;
                                if (heightMultiple != 0)
                                {
                                    maxHeightMultiple = heightMultiple;
                                }
                                activitycell.CellStyle = smallWrapStyle;

                                postcell.SetCellValue(eca.post);
                                postcell.CellStyle = smallWrapStyle;
                                heightMultiple = eca.post.Length / 27;
                                if (heightMultiple > maxHeightMultiple)
                                {
                                    maxHeightMultiple = heightMultiple;
                                }
                                
                                achievementcell.SetCellValue(eca.achievement);
                                achievementcell.CellStyle = smallWrapStyle;
                                heightMultiple = eca.achievement.Length/70;
                                if (heightMultiple > maxHeightMultiple)
                                {
                                    maxHeightMultiple = heightMultiple;
                                }

                                if (maxHeightMultiple != 0)
                                {
                                    row.HeightInPoints = 24 * (maxHeightMultiple + 1);
                                }
                                ecacount++;
                            }
                            shiftedrows += ecacount;
                            shiftedrows += 1;
                            rowcount++;
                        }

                        // handle remarks
                        var remarkrow = sheet.CreateRow(rowcount++);
                        var remarkstartrow = rowcount;
                        var remarkEntryRow = sheet.CreateRow(rowcount++);
                        sheet.ShiftRows(60, 67, 0 - (60 - shiftedrows));
                        shiftedrows += 7;
                        rowcount += 5; // 2 rows created already above

                        sheet.AddMergedRegion(new CellRangeAddress(remarkstartrow, remarkstartrow + 1, 0, 7)); // remark

                        var studentremark = student.students_remarks.SingleOrDefault(x => x.term == term.Value && x.year == year);
                        if (studentremark != null)
                        {
                            var remarkcell = remarkEntryRow.CreateCellIfNotExist(0);
                            remarkcell.SetCellValue(studentremark.remarks);
                            remarkcell.CellStyle = smallWrapStyle;

                            // handle conduct
                            var conductCell = remarkEntryRow.CreateCellIfNotExist(10);
                            conductCell.SetCellValue(studentremark.conduct);
                        }

                        var term_info = repository.GetAttendanceTerm((int) _school, term.Value, year);
                        if (term_info != null)
                        {
                            var totalLate = student.GetAttendanceCount(term_info.startdate, term_info.enddate, AttendanceStatus.ABSENT, true);
                            var attendanceCell = remarkEntryRow.CreateCellIfNotExist(8);
                            attendanceCell.SetCellValue(string.Concat(term_info.days - totalLate, " / ", term_info.days));
                        }

                        remarkrow.HeightInPoints = 27; // the title

                        // handle signature
                        var sigrow = sheet.CreateRow(rowcount++);
                        sheet.ShiftRows(70, 70, 0 - (70 - shiftedrows));
                        sigrow.HeightInPoints = 33;
                    }
                    
                    // save
                    templateWorkbook.RemoveSheetAt(0);
                    templateWorkbook.Write(ms);
                }
#if !DEBUG
                catch (Exception ex)
                {
                    Syslog.Write(ex);
                }
#endif
            }
            var filename = usr.ToName();
            if (allclass)
            {
                filename = student_class.school_class.name;
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("ReportCard_{0}_{1}.xls", year, filename));
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE)]
        public ActionResult Save(string classes, string exam_date, string exam_name, int selector_school, int? selector_template, string exam_remarks)
        {
            // validate data
            if (string.IsNullOrEmpty(classes))
            {
                return Json("No classes selected".ToJsonFail());
            }

            if (!selector_template.HasValue)
            {
                return Json("No template selected".ToJsonFail());
            }

            var exam = new exam();
            exam.name = exam_name;
            exam.description = exam_remarks;
            exam.creator = sessionid.Value;

            if (string.IsNullOrEmpty(exam_date))
            {
                exam.exam_date = Utility.GetDBDate();
            }
            else
            {
                exam.exam_date = DateTime.ParseExact(exam_date, Constants.DATEFORMAT_DATEPICKER, CultureInfo.InvariantCulture);
            }
            exam.year = exam.exam_date.Year;
            exam.schoolid = selector_school;

            var examclasses = classes.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries).Select(int.Parse);

            // get template
            var template = repository.GetExamTemplate(selector_template.Value);
            
            // get subjects from template
            var examsubjects = template.exam_template_subjects.OrderBy(x => x.position);

            // set maxmark
            exam.maxMark = template.maxMark;

            foreach (var examclass in examclasses)
            {
                exam.exam_classes.Add(new exam_class(){ classid = examclass});
            }

            foreach (var subject in examsubjects)
            {
                exam.exam_subjects.Add(new exam_subject()
                                           {name = subject.name, code = subject.code, position = subject.position, subjectid = subject.subjectid});
            }

            try
            {
                repository.AddExam(exam);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json(exam.id.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult SaveDescription(long id, string remarks)
        {
            var exam = repository.GetExam(id);
            if (exam == null)
            {
                return Json("Exam not found".ToJsonFail());
            }

            if (sessionid.Value != exam.creator)
            {
                return Json("Only creator can update the description".ToJsonFail());
            }

            exam.description = remarks;
            repository.Save();
            return Json("Description updated".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE | Permission.TRANSCRIPTS_EDIT)]
        public ActionResult SaveRemark(long studentid, short? term, int year, string remarks, string conduct)
        {
            if (!term.HasValue)
            {
                return Json("Please specify school term".ToJsonOKMessage());
            }
            var usr = repository.GetUser(studentid);
            var dbRemark = usr.students_remarks.SingleOrDefault(x => x.term == term.Value && x.year == year);
            if (dbRemark == null)
            {
                dbRemark = new students_remark();
                dbRemark.year = year;
                dbRemark.term = term.Value;
                usr.students_remarks.Add(dbRemark);

            }
            var entry_name = "";
            if (remarks != null)
            {
                dbRemark.remarks = remarks.Trim();
                entry_name = "Remarks";
            }
            if (conduct != null)
            {
                if (conduct.Length > 1)
                {
                    return Json("Conduct too long".ToJsonFail());
                }
                dbRemark.conduct = conduct;
                entry_name = "Conduct";
            }
            repository.Save();

            return Json(string.Format("{0} saved successfully", entry_name).ToJsonOKMessage());
        }

        /// <summary>
        /// used to list exams to be included in report card
        /// </summary>
        /// <param name="studentid"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.TRANSCRIPTS_CREATE)]
        public ActionResult Select(long studentid, int year)
        {
            var usr = repository.GetUser(studentid);
            var exams = usr.exam_marks
                .Where(x => x.exam.year == year)
                .Select(x => x.exam)
                .Distinct()
                .OrderBy(x => x.name)
                .Select(x => new { x.id, x.name});

            return Json(exams.ToJsonOKData());
        }

        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Show(long id)
        {
            var canview = false;
            var usr = repository.GetUser(id);
            if (sessionid.HasValue)
            {
                canview = usr.GetCanView(sessionid.Value, auth);
            }

            if (!canview)
            {
                return new EmptyResult();
            }

            var viewmodel = new ExamStudentViewModel(usr, usr.exam_marks.AsQueryable(), DateTime.Now.Year);
            return View(viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_VIEW | Permission.EXAM_EDIT)]
        public ActionResult Single(long id)
        {
            var exam = repository.GetExam(id);
            if (exam == null)
            {
                return ReturnNotFoundView();
            }

            var viewmodel = new ExamViewModel(baseviewmodel);
            viewmodel.exam = exam.ToMarksModel();
            viewmodel.description = exam.description;
            viewmodel.iscreator = sessionid.Value == exam.creator;

            // handle exam subjects
            viewmodel.examsubjects = exam.exam_subjects
                .OrderBy(x => x.position)
                .Select(x => new ExamTemplateSubjectViewModel()
                                 {
                                     id = x.id.ToString(),
                                     examsubjectname = x.name,
                                     subjects =
                                         new[] {new SelectListItem() {Text = "None", Value = ""}}.Union(
                                             exam.school.subjects.Select(y => new SelectListItem()
                                                                                  {
                                                                                      Text = y.name,
                                                                                      Value = y.id.ToString(),
                                                                                      Selected = y.id == x.subjectid
                                                                                  }))
                                 });

            viewmodel.subjects = exam.exam_subjects.OrderBy(x => x.name).Select(x => new Subject(x.id, x.name, x.position));


            viewmodel.class_names = exam.exam_classes.Select(x => x.school_class.name).ToArray();
            if (exam.exam_classes.First().school_class.school_year.grades_method != null)
            {
                viewmodel.grades = exam.exam_classes.First()
                    .school_class
                    .school_year
                    .grades_method
                    .grades_rules
                    .OrderByDescending(x => x.mark)
                    .Select(x => x.grade).ToArray();
            }

            foreach (var subject in viewmodel.subjects)
            {
                // storage for parsed marks, key = class, value = list of grades
                var parsedmarks = new Dictionary<string, List<string>>();
                var exam_subject = subject;

                // then by class
                foreach (var examClass in exam.exam_classes)
                {
                    var class_name = examClass.school_class.name;

                    // get all marks for this class for this subject
                    var marks = exam.exam_marks.Where(x => x.exam_subjectid == exam_subject.id &&
                        x.user.classes_students_allocateds
                        .First(y => y.year == exam.exam_date.Year)
                        .school_class.name == class_name);

                    var grades = new List<string>();
                    foreach (var mark in marks)
                    {
                        short markval;
                        if (short.TryParse(mark.mark, out markval) && !mark.absent)
                        {
                            var parsedgrade = markval.ToGrade(examClass.school_class.school_year);
                            grades.Add(parsedgrade.grade);
                        }
                        else
                        {
                            if (mark.absent)
                            {
                                grades.Add("X");
                            }
                            else
                            {
                                // add grade
                                grades.Add(mark.mark);
                            }
                        }
                    }
                    parsedmarks.Add(class_name,grades);
                } // end class loop

                // if only grades were recorded and not marks, then we just count the number of grades
                if (!viewmodel.grades.Any())
                {
                    viewmodel.grades = parsedmarks.Values.SelectMany(x => x).Distinct().ToArray();
                }


                // calculate stats
                var stats = new List<ExamStat>();
                foreach (var entry in viewmodel.grades)
                {
                    var grade = entry;
                    foreach (var @class in viewmodel.class_names)
                    {
                        var marks = parsedmarks[@class];
                        var total = marks.Count;
                        var count = marks.Count(x => String.Compare(x, grade, StringComparison.OrdinalIgnoreCase) == 0);
                        var stat = new ExamStat
                                       {
                                           class_name = @class,
                                           count = count,
                                           grade = grade,
                                           percentage = (count == 0 || total == 0)?"":((double) count/total*100).ToString("n2") + "%"
                                       };
                        // add to stats table
                        stats.Add(stat);
                    }
                }
                viewmodel.stats.Add(subject.name,stats);
            
            } // end subject loop
            
            return View(viewmodel);
        }

        /// <summary>
        /// ordering subjects in exam results
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult OrderSubject(long id, string ids)
        {
            ids = ids.Replace("subject[]=", "");
            var subjects = ids.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var exam = repository.GetExam(id);
            if (exam == null)
            {
                return Json("Exam not found".ToJsonFail());
            }

            if (exam.creator != sessionid.Value)
            {
                return Json("Only the creator can update exam settings".ToJsonFail());
            }

            byte count = 1;
            foreach (var entry in subjects)
            {
                var subject = exam.exam_subjects.SingleOrDefault(x => x.id.ToString() == entry);
                if (subject != null)
                {
                    subject.position = count++;
                }
            }
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Order updated successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [ExamJsonFilter(Param = "data")]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_EDIT)]
        public ActionResult Update(long id, StudentMarkJsonModel[] data, string inputjson)
        {
            // if the creator is a teacher then only allow the teacher to modify the marks
            var exam = repository.GetExam(id);
            if (exam == null)
            {
                return Json("Exam not found".ToJsonFail());
            }
            
            foreach (var student in data)
            {
                foreach (var mark in student.marks)
                {
                    var emark = repository.GetExamMark(id, student.id, mark.subjectid);
                    if (emark != null)
                    {
                        // check that person can modify mark
                        var subjectClassString = repository.CanModifyExamMark(sessionid.Value, emark.exam_subject.id,
                                                                       emark.studentid, exam.year);
                        if (!string.IsNullOrEmpty(subjectClassString))
                        {
                            return Json(string.Format("Save failed. <strong>{0}</strong> is restricted.", subjectClassString).ToJsonFail());
                        }
                        
                        if (!string.IsNullOrEmpty(mark.mark))
                        {
                            short m;
                            if (short.TryParse(mark.mark, out m))
                            {
                                // this is actually mark
                                // don't allow mark greater than max mark
                                if (m > exam.maxMark)
                                {
                                    return Json("Mark is higher than maximum allowed".ToJsonFail());
                                }

                                emark.mark = mark.mark;
                                emark.absent = false;
                            }
                            else
                            {
                                if (mark.mark.IsKindyGrade() ||
                                    ExamMark.AllowedCharacters.Contains(mark.mark.ToLowerInvariant()))
                                {
                                    emark.mark = mark.mark;
                                    emark.absent = false;
                                }
                                else if (string.Compare(mark.mark, "x", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    emark.absent = true;
                                }
                                else
                                {
                                    return SendJsonErrorResponse("Unsupported mark: " + mark.mark);
                                }
                            }
                        }
                        else
                        {
                            repository.DeleteExamMark(emark);
                        }
                    }
                    else
                    {
                        // check if person can modify exam mark
                        var subjectClassString = repository.CanModifyExamMark(sessionid.Value, mark.subjectid, student.id,
                                                                       exam.year);
                        if (!string.IsNullOrEmpty(subjectClassString))
                        {
                            return Json(string.Format("Save failed. <strong>{0}</strong> is restricted.", subjectClassString).ToJsonFail());
                        }

                        if (!string.IsNullOrEmpty(mark.mark))
                        {
                            emark = new exam_mark();

                            short m;
                            if (short.TryParse(mark.mark, out m))
                            {
                                // this is atually mark
                                // don't allow mark greater than max mark
                                if (m > exam.maxMark)
                                {
                                    return Json("Mark is higher than maximum allowed".ToJsonFail());
                                }

                                emark.mark = mark.mark;
                                emark.absent = false;
                            }
                            else
                            {
                                if (mark.mark.IsKindyGrade() ||
                                    ExamMark.AllowedCharacters.Contains(mark.mark.ToLowerInvariant()))
                                {
                                    emark.mark = mark.mark;
                                    emark.absent = false;
                                }
                                else if (string.Compare(mark.mark, "x", StringComparison.InvariantCultureIgnoreCase) == 0)
                                {
                                    emark.absent = true;
                                }
                                else
                                {
                                    return SendJsonErrorResponse("Unsupported mark: " + mark.mark);
                                }
                            }
                            emark.studentid = student.id;
                            emark.exam_subjectid = mark.subjectid;
                            emark.examid = id;
                            repository.AddExamMark(emark);
                        }
                    }
                }
            }

            try
            {
                // log subjects that were changed
                var subjectids = data.SelectMany(x => x.marks).Select(y => y.subjectid).Distinct().ToArray();
                var subjectNames = repository.GetExamSubjects(subjectids);

                var change = new changelog();
                change.changes = string.Format("Exam {0}({1}): {2}", 
                                                    exam.id, 
                                                    exam.name, 
                                                    string.Join(",", subjectNames.ToArray()));
                change.created = DateTime.Now;
                change.userid = sessionid.Value;

                repository.AddChangeLog(change);
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ErrorLevel.ERROR, "Exam/Update JSON: " + inputjson);
                return SendJsonErrorResponse(ex);
            }

            return Json("Marks updated successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Settings(long id, long[] subjectid, int?[] subject, short maxMark)
        {
            // only allow admin and exam creators to update setting
            var exam = repository.GetExam(id);
            exam.maxMark = maxMark;

            if (exam.creator != sessionid.Value)
            {
                return Json("Only creator can update exam settings".ToJsonFail());
            }

            for (int i = 0; i < subjectid.Length; i++)
            {
                var sid = subject[i];

                var examsubject = exam.exam_subjects.Single(x => x.id == subjectid[i]);
                examsubject.subjectid = sid;
            }

            repository.Save();

            return Json("Exam settings updated".ToJsonOKMessage());
        }
    }
}
