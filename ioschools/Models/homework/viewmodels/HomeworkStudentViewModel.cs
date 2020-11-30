using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.DB;
using ioschools.DB.repository;

namespace ioschools.Models.homework.viewmodels
{
    public class HomeworkStudentViewModel
    {
        public long studentid { get; set; }
        public IEnumerable<HomeworkStudent> homeworks { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }
        public IEnumerable<SelectListItem> subjects { get; set; } 

        public HomeworkStudentViewModel()
        {
            homeworks = Enumerable.Empty<HomeworkStudent>();
        }

        public HomeworkStudentViewModel(long id, IQueryable<classes_students_allocated> classes, int year, bool canUpload) : this()
        {
            studentid = id;
            var allocated = classes.SingleOrDefault(x => x.year == year);
            if (allocated != null)
            {
                int year1 = year;
                homeworks = allocated.school_class.homework_students
                    .Where(x => x.homework.created.Year == year1 && x.studentid == id)
                    .OrderByDescending(x => x.homework.created)
                    .ToModel(canUpload);
            }

            int finalYear = DateTime.Now.Year;
            using (var repository = new Repository())
            {
                var student = repository.GetUser(id);
                if (student.FinalYear.HasValue)
                {
                    finalYear = student.FinalYear.Value;
                    if (year > finalYear)
                    {
                        year = finalYear;
                    }
                }
            }

            years =
                classes.SelectMany(x => x.school_class.homework_students)
                    .Select(x => x.homework.created.Year).Union(new[] {finalYear})
                    .Distinct()
                    .Where(x => x <= finalYear) // don't show stuff afgter student has left
                    .Select(x => new SelectListItem() {Text = x.ToString(), Value = x.ToString(), Selected = x == year});


        }
    }
}