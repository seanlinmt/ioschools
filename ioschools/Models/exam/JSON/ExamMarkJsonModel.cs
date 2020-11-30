using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.exam.JSON
{
    public class ExamMarkJsonModel
    {
        public long? id { get; set; }
        public long subjectid { get; set; }
        public string mark { get; set; }
    }
}