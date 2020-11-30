using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.exam
{
    public class Grade
    {
        public string grade { get; set; }
        public decimal? gradepoint { get; set; }

        public Grade(string grade, decimal? score = null)
        {
            this.grade = grade;
            this.gradepoint = score;
        }

        public Grade()
        {
            this.grade = "";
        }
    }
}