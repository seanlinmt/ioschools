using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.User;

namespace ioschools.Models.eca
{
    public class ECAStudentViewModel
    {
        public long studentid { get; set; }
        public IEnumerable<ECAStudent> ecas { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }
        public bool canEdit { get; set; }

        public ECAStudentViewModel(ioschools.DB.user student, Permission perms, int year)
        {
            ecas = student.eca_students.Where(x => x.year == year).OrderBy(x => x.eca.name).ToModel();
            
            studentid = student.id;
            years =
                student.eca_students.Select(x => x.year).Union(new[] { DateTime.Now.Year }).
                    Distinct().OrderByDescending(x => x).Select(
                        x =>
                        new SelectListItem() { Text = x.ToString(), Value = x.ToString(), Selected = (x == year) });
            canEdit = perms.HasFlag(Permission.ECA_CREATE);
        }
    }
}