using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.DB;

namespace ioschools.Models.user.student
{
    public class Testimonial
    {
        public string student_name { get; set; }
        public long studentid { get; set; }
        public string test_academic { get; set; }
        public string test_diligence { get; set; }
        public string test_attendance { get; set; }
        public string test_responsible { get; set; }
        public string test_initiative { get; set; }
        public string test_conduct { get; set; }
        public string test_honesty { get; set; }
        public string test_reliance { get; set; }
        public string test_collaborate { get; set; }
        public string test_appearance { get; set; }
        public string test_bm { get; set; }
        public string test_english { get; set; }
        public string test_remarks { get; set; }
    }

    public static class TestimonialHelper
    {
        public static Testimonial ToTestimonialModel(this registration row)
        {
            return new Testimonial()
                       {
                           student_name = row.user.ToName(),
                           studentid = row.studentid,
                           test_academic = row.test_academic,
                           test_diligence = row.test_diligence,
                           test_attendance = row.test_attendance,
                           test_responsible = row.test_responsible,
                           test_initiative = row.test_initiative,
                           test_conduct = row.test_conduct,
                           test_honesty = row.test_honesty,
                           test_reliance = row.test_reliance,
                           test_collaborate = row.test_collaborate,
                           test_appearance = row.test_appearance,
                           test_bm = row.test_bm,
                           test_english = row.test_english,
                           test_remarks = row.test_remarks
                       };
        }
    }
}