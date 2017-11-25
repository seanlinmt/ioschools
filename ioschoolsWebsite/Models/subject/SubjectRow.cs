using System.Collections.Generic;
using System.Web.Mvc;
using ioschoolsWebsite.Models.subject.viewmodels;

namespace ioschoolsWebsite.Models.subject
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