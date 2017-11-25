using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.exam.JSON
{
    public class StudentMarkJsonModel
    {
        public long id { get; set; }
        public IEnumerable<ExamMarkJsonModel> marks { get; set; }
    }
}