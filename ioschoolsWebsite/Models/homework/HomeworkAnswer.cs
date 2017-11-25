using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.homework
{
    public class HomeworkAnswer
    {
        public string studentname { get; set; }
        public long studentid { get; set; }
        public string classname { get; set; }
        public IEnumerable<HomeworkAnswerFile> files { get; set; } 
    }
}