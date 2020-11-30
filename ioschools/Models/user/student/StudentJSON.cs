using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.user.student
{
    public class StudentJSON : IdName
    {
        public string classname { get; set; }
        public int classid { get; set; }
        public string imageUrl { get; set; }
    }
}