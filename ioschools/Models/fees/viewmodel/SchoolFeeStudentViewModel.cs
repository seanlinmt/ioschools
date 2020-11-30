using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.DB;

namespace ioschools.Models.fees.viewmodel
{
    public class SchoolFeeStudentViewModel
    {
        public long studentid { get; set; }
        public IEnumerable<SelectListItem> yearList { get; set; }

        public IEnumerable<SchoolFeeStudent> fees { get; set; }

        public bool canEdit { get; set; }

        private SchoolFeeStudentViewModel()
        {
            yearList = Enumerable.Empty<SelectListItem>();
            fees = Enumerable.Empty<SchoolFeeStudent>();
        }

        public SchoolFeeStudentViewModel(ioschools.DB.user student, Permission perm, int year) : this()
        {
            studentid = student.id;
            canEdit = perm.HasFlag(Permission.FEES_ADMIN);

            if (student.classes_students_allocateds.Any())
            {
                int finalYear = DateTime.Now.Year;

                // now see if the student has left
                if (student.FinalYear.HasValue)
                {
                    finalYear = student.FinalYear.Value;
                    if (year > finalYear)
                    {
                        year = finalYear;
                    }
                }

                yearList = student.classes_students_allocateds.Select(x => x.year)
                    .Distinct()
                    .Where(x => x <= finalYear) // don't show stuff after student has left
                    .OrderByDescending(x => x)
                    .Select(x => new SelectListItem()
                                     {
                                         Text = x.ToString(),
                                         Value = x.ToString(),
                                         Selected = x == year
                                     });

                fees = student.fees.Where(x => x.duedate.Year == year).ToModel(perm);
            }
        }
    }
}