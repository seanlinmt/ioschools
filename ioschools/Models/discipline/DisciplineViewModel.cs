using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.User;

namespace ioschools.Models.discipline
{
    public class DisciplineViewModel
    {
        public IEnumerable<Discipline> disciplines { get; set; }
        public bool hideCreator { get; set; }
        public int totalDemerit { get; set; }
        public int totalMerit { get; set; }
        public int totalPoints
        {
            get { return totalMerit + totalDemerit; }
        }
        public long studentid { get; set; }
        public IEnumerable<SelectListItem> years { get; set; }

        public DisciplineViewModel(ioschools.DB.user u, long viewerId, UserGroup viewerUsergroup, int year)
        {
            studentid = u.id;
            disciplines = u.students_disciplines.Where(x => x.created.Year == year).ToModel(viewerId).ToArray();

            if (viewerUsergroup == UserGroup.GUARDIAN ||
                viewerUsergroup == UserGroup.STUDENT)
            {
                hideCreator = true;
            }
            totalDemerit = disciplines.Where(x => x.points.HasValue && x.points.Value < 0).Sum(x => x.points.Value);
            totalMerit = disciplines.Where(x => x.points.HasValue && x.points.Value > 0).Sum(x => x.points.Value);
            years =
                u.students_disciplines.Select(x => x.created.Year).Union(new[] { year }).
                    Distinct().OrderByDescending(x => x).Select(
                        x =>
                        new SelectListItem() {Text = x.ToString(), Value = x.ToString(), Selected = (x == year)});
        }
    }
}