using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Models.subject
{
    public class SubjectTeacher
    {
        public string subjectname { get; set; }
        public int year { get; set; }
        public long subjectid { get; set; }
        public List<SubjectTeacherEntry> teachers { get; set; }

        public SubjectTeacher()
        {
            teachers = new List<SubjectTeacherEntry>();
        }
    }
}