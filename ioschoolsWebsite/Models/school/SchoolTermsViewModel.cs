using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.school
{
    public class SchoolTermsViewModel
    {
        public IEnumerable<SchoolTerm> terms { get; set; }
        public int schoolid { get; set; }
        public string schoolname { get; set; }
    }
}