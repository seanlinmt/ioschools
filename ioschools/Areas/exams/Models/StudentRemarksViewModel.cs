using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;

namespace ioschools.Areas.exams.Models.remarks
{
    public class StudentRemarksViewModel
    {
        public long studentid { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }
        public int year { get; set; }
        public bool canEdit { get; set; }

        public StudentRemarksViewModel(ioschools.DB.user student, Permission viewer_perm, int year)
        {
            studentid = student.id;

            int finalYear = DateTime.Now.Year;
            if (student.FinalYear.HasValue)
            {
                finalYear = student.FinalYear.Value;
                if (year > finalYear)
                {
                    year = finalYear;
                }
            }
            
            this.year = year;

            years =
                student.classes_students_allocateds
                .Select(x => x.year).Union(new[] { finalYear })
                .Distinct()
                .Where(x => x <= finalYear) // don't show stuff afgter student has left
                .Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString(), Selected = x == year});

            canEdit = (viewer_perm & (Permission.TRANSCRIPTS_CREATE | Permission.TRANSCRIPTS_EDIT)) != 0;
        }

    }
}