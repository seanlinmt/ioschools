using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.school
{
    public class AdminSchoolClassesViewModel
    {
        public string schoolname { get; set; }
        public int schoolid { get; set; }

        // years
        public IEnumerable<SchoolYear> schoolYears { get; set; } 

        // classes
        public IEnumerable<SchoolClass> classes { get; set; }

    }
}