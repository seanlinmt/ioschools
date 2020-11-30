using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.DB;

namespace ioschools.Models.classes
{
    public class AllocatedStudent
    {
        public long id { get; set; }
        public int year { get; set; }
        public string school { get; set; }
        public string classname { get; set; }
    }

    public static class AllocatedStudentHelper
    {
        public static IEnumerable<AllocatedStudent> ToModel(this IEnumerable<classes_students_allocated> rows)
        {
            foreach (var row in rows)
            {
                yield return new AllocatedStudent()
                                 {
                                     id = row.id,
                                     year = row.year,
                                     school = row.school_class.school.name,
                                     classname = row.school_class.name
                                 };
            }
        }

    }
}