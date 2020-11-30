using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.DB;

namespace ioschools.Models.exam
{
    public class ExamStudentViewModel
    {
        public long studentid { get; set; }
        public int year { get; set; }
        public List<SelectListItem> years { get; set; }

        public ExamStudentViewModel(ioschools.DB.user student, IQueryable<exam_mark> examMarks, int year)
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
                examMarks
                .Select(x => x.exam.year).Union(new[] { finalYear })
                .Distinct()
                .Where(x => x <= finalYear)
                .Select(x => new SelectListItem() { Text = x.ToString(), Value = x.ToString(), Selected = x == year})
                .ToList();
        }
    }
}