using System.Collections.Generic;
using System.Web.Mvc;
using ioschools.Models.subject.viewmodels;

namespace ioschools.Models.subject
{
    public class SubjectRow
    {
        public AdminSubject subject { get; set; }
        public IEnumerable<SelectListItem> schoolList { get; set; }

        public SubjectRow()
        {
            subject = new AdminSubject();
        }
    }
}