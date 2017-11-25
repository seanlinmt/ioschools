using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.subject.viewmodels
{
    public class SubjectsTeaching
    {
        public int year { get; set; }
        public string school { get; set; }
        public string subjectname { get; set; }
        public string classesTeaching { get; set; }
    }
}