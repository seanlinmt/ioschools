using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.Attendance;
using ioschools.Data.User;

namespace ioschools.Models.attendance
{
    public class AttendanceViewModel
    {
        public long studentid { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }
        public IEnumerable<Attendance> attendances { get; set; }
        public bool canEdit { get; set; }
        public int class_absent { get; set; }
        public int class_late { get; set; }
        public int eca_absent { get; set; }
        public int eca_late { get; set; }

        public AttendanceViewModel()
        {
            attendances = Enumerable.Empty<Attendance>();
            canEdit = false;
        }

        public void Initialise(ioschools.DB.user student, UserGroup viewer_usergroup, int year)
        {
            attendances =
                student.attendances.Where(x => x.date.Year == year).OrderByDescending(
                    x => x.date).ToModel();
            studentid = student.id;
            years =
                student.attendances.Select(x => x.date.Year).
                    Distinct().OrderBy(x => x).Select(
                        x =>
                        new SelectListItem() {Text = x.ToString(), Value = x.ToString(), Selected = (x == year)});
            
            // get totals for the year
            class_absent = student.GetAttendanceCount(year, AttendanceStatus.ABSENT, true);
            class_late = student.GetAttendanceCount(year, AttendanceStatus.LATE, true);
            eca_absent = student.GetAttendanceCount(year, AttendanceStatus.ABSENT, false);
            eca_late = student.GetAttendanceCount(year, AttendanceStatus.LATE, false);

            if (UserSuperGroups.STAFF.HasFlag(viewer_usergroup))
            {
                canEdit = true;
            }
        }

    }
}