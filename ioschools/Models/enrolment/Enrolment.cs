using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.DB;
using ioschools.DB.repository;
using ioschools.Library;
using ioschools.Library.Helpers;
using ioschools.Library.Imaging;
using ioschools.Models.user;
using clearpixels.Models.jqgrid;

namespace ioschools.Models.enrolment
{
    public class Enrolment
    {
        public string student_name { get; set; }
        public long id { get; set; }
        public string status { get; set; }
        public string school { get; set; }
        public string school_year { get; set; }
        public IEnumerable<IdNameUrl> attachments { get; set; }

        public List<SelectListItem> enrol_schoolList { get; set; }
        public List<SelectListItem> enrol_yearList { get; set; }
        public List<SelectListItem> enrol_statusList { get; set; }
        public string year { get; set; }


        public DateTime? admissionDate { get; set; }
        public IEnumerable<SelectListItem> admissionDayList { get; set; }
        public IEnumerable<SelectListItem> admissionMonthList { get; set; }

        public DateTime? leftDate { get; set; }
        public IEnumerable<SelectListItem> leftDayList { get; set; }
        public IEnumerable<SelectListItem> leftMonthList { get; set; }

        public string leaving_reason { get; set; }
        public string previous_class { get; set; }
        public string previous_school { get; set; }
        public string disability_details { get; set; }
        public bool? handicap { get; set; }
        public bool? learning_problems { get; set; }

        public Enrolment()
        {
            attachments = Enumerable.Empty<IdNameUrl>();
            admissionDayList = DateHelper.GetDayList(null, true);
            admissionMonthList = DateHelper.GetMonthList(null, true);
            leftDayList = DateHelper.GetDayList(null, true);
            leftMonthList = DateHelper.GetMonthList(null, true);
            enrol_schoolList = new List<SelectListItem>();
            enrol_yearList = new List<SelectListItem>();
            enrol_statusList = new List<SelectListItem>();
        }

        public void InitSchoolList(int? val = null)
        {
            enrol_schoolList.Add(new SelectListItem() { Text = "select school", Value = "" });
            enrol_schoolList.Add(new SelectListItem() { Text = " Kindergarten", Value = "1", Selected = val == 1 });
            enrol_schoolList.Add(new SelectListItem() { Text = " Primary", Value = "2", Selected = val == 2 });
            enrol_schoolList.Add(new SelectListItem() { Text = " Secondary", Value = "3", Selected = val == 3 });
            enrol_schoolList.Add(new SelectListItem() { Text = " International", Value = "4", Selected = val == 4 });
        }

        public void InitSchoolYearList(int? school_val = null, int? year_val = null)
        {
            if (!school_val.HasValue)
            {
                return;
            }
            using (var repo = new Repository())
            {
                var years = repo.GetSchoolYears().Where(x => x.schoolid == school_val.Value);
                foreach (var entry in years)
                {
                    enrol_yearList.Add(new SelectListItem(){Text = entry.name, Value = entry.id.ToString(), Selected = entry.id == year_val});
                }
            }
        }

        public void InitStatusList(string value = "")
        {
            foreach (var entry in Enum.GetValues(typeof(RegistrationStatus)))
            {
                var val = entry.ToString();
                enrol_statusList.Add(new SelectListItem(){Text = val, Value = val, Selected = val == value});
            }
        }
    }

    public static class EnrolmentHelper
    {
        private static string ToEnrolGridActions(this registration row)
        {
            return string.Format(
                    "<a class='jqedit' href='/enrolment/edit/{0}'>edit</a><a class='jqdelete' href='#'>del</a>",
                    row.id);

        }

        public static JqgridTable ToEnrolJqGrid(this IEnumerable<registration> rows)
        {
            var grid = new JqgridTable();
            foreach (var row in rows)
            {
                var entry = new JqgridRow();
                entry.id = row.id.ToString();
                entry.cell = new object[]
                                 {
                                     row.id,
                                     row.user.photo.HasValue
                                         ? Img.by_size(row.user.user_image.url, Imgsize.USER_THUMB).ToHtmlImage()
                                         : Img.PHOTO_NO_THUMBNAIL.ToHtmlImage(),
                                     row.user.ToJqName(),
                                     row.created.ToString(Constants.DATETIME_SHORT_DATE),
                                     row.ToEnrollingSchool(),
                                     row.enrollingYear.HasValue?row.enrollingYear.Value.ToString():"",
                                     row.ToStatus(),
                                     row.ToEnrolGridActions()
                                 };
                grid.rows.Add(entry);
            }
            return grid;
        }

        private static string ToEnrollingSchool(this registration row)
        {
            return string.Format("<div>{0}</div><div>{1}</div>", row.schoolid.HasValue?row.school.name:"", row.schoolyearid.HasValue?row.school_year.name:"");
        }

        public static Enrolment ToModel(this registration row)
        {
            var e = new Enrolment();

            if (row == null)
            {
                return e;
            }

            e.id = row.id;
            e.status = row.ToStatus();
            e.student_name = row.user.ToName();
            e.school = row.schoolid.HasValue?row.school.name:"";
            e.school_year = row.schoolyearid.HasValue?row.school_year.name:"";
            e.attachments =
                row.user.user_files.Select(x => new IdNameUrl()
                {
                    id = x.id.ToString(),
                    name = x.filename,
                    url = x.url
                });

            e.InitSchoolList(row.schoolid);
            e.InitSchoolYearList(row.schoolid,row.schoolyearid);
            e.InitStatusList(row.status);
            e.year = row.enrollingYear.HasValue
                            ? row.enrollingYear.Value.ToString()
                            : "";

            e.previous_class = row.previous_class;
            e.previous_school = row.previous_school;
            e.admissionDate = row.admissionDate;
            e.leftDate = row.leftDate;
            e.leaving_reason = row.leaving_reason;
            e.handicap = row.isHandicap;
            e.learning_problems = row.hasLearningProblem;
            e.disability_details = row.disability_details;

            if (e.admissionDate.HasValue)
            {
                e.admissionDayList = DateHelper.GetDayList(e.admissionDate.Value.Day, true);
                e.admissionMonthList = DateHelper.GetMonthList(e.admissionDate.Value.Month, true);
            }

            if (e.leftDate.HasValue)
            {
                e.leftDayList = DateHelper.GetDayList(e.leftDate.Value.Day, true);
                e.leftMonthList = DateHelper.GetMonthList(e.leftDate.Value.Month, true);
            }

            return e;
        }

        public static IEnumerable<Enrolment> ToModel(this IEnumerable<registration> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }

        private static string ToStatus(this registration row)
        {
            var status = row.status.ToEnum<RegistrationStatus>();
            switch (status)
            {
                case RegistrationStatus.PENDING:
                    return "<span class='tag_orange'>pending</span>";
                case RegistrationStatus.ACCEPTED:
                    return "<span class='tag_green'>accepted</span>";
                case RegistrationStatus.REJECTED:
                    return "<span class='tag_red'>rejected</span>";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}