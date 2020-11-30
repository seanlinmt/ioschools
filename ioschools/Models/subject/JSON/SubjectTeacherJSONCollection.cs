using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.subject.JSON
{
    public class SubjectTeacherJSONCollection
    {
        public int year { get; set; }
        public int subjectid { get; set; }
        public IEnumerable<SubjectTeacherJSON> teachers { get; set; } 
    }
}