using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Library;
using ioschools.Models.user;
using ioschools.Models.user.student;

namespace ioschools.Models.homework
{
    public class Homework
    {
        public long contextid { get; set; }
        public string created { get; set; }
        public string id { get; set; }
        public string message { get; set; }
        public string title { get; set; }
        public IEnumerable<SelectListItem> classList { get; set; }
        public IEnumerable<SelectListItem> subjectList { get; set; } 
        public IEnumerable<IdName> classes { get; set; }
        public IEnumerable<IdNameUrl> files { get; set; }
        public float totalSize { get; set; }
        public bool editmode { get; set; }
        public string subjectname { get; set; }
        public bool notifyme { get; set; }
        
        public Homework()
        {
            classes = Enumerable.Empty<IdName>();
            files = Enumerable.Empty<IdNameUrl>();
            classList = Enumerable.Empty<SelectListItem>();
            contextid = DateTime.Now.Ticks;
        }
    }

    public static class HomeworkHelper
    {
        public static Homework ToModel(this ioschools.DB.homework row, bool editable, int year)
        {
            return new Homework()
                       {
                           created = row.created.ToString(Constants.DATETIME_STANDARD),
                           id = row.id.ToString(),
                           classList =
                               row.user.subject_teachers.Where(x => x.year == year)
                               .Select(
                                   x => new SelectListItem() {Text = x.school_class.name, Value = x.classid.ToString()}),
                           classes =
                               row.homework_students.GroupBy(x => x.school_class).Select(
                                   x => new IdName(x.Key.id, x.Key.name)),
                           files =
                               row.homework_files.Select(
                                   x => new IdNameUrl() {id = x.id.ToString(), name = x.filename, url = x.url}),
                           message = row.message,
                           title = row.title,
                           subjectList = row.user.subject_teachers.Where(x => x.year == year)
                               .Select(x => x.subject)
                               .Distinct()
                               .Select(x => new SelectListItem()
                                                {
                                                    Text = x.name,
                                                    Value = x.id.ToString()
                                                }),
                           totalSize = row.homework_files.Sum(x => x.size),
                           subjectname = row.subject.name,
                           notifyme = row.notifyme
                       };
        }

        public static IEnumerable<Homework> ToModel(this IEnumerable<ioschools.DB.homework> rows, bool editable, int year)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel(editable, year);
            }
        }
    }
}