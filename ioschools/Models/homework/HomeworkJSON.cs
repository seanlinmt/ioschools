using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Models.user.student;

namespace ioschools.Models.homework
{
    public class HomeworkJSON
    {
        public string id { get; set; }
        public long subject { get; set; }
        public string message { get; set; }
        public string title { get; set; }
        public IEnumerable<long> students { get; set; }
        public IEnumerable<IdNameUrl> files { get; set; }
        public bool notifyme { get; set; }
    }
}