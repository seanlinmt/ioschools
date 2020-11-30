using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;

namespace ioschools.Models.user
{
    public class UserChild : IdName
    {
        public long linkid { get; set; } // student_guardian id
        public string class_name { get; set; }
        public string relationship { get; set; }
    }

    public static class UserChildHelper
    {
        public static IEnumerable<UserChild> ToChildrenModel(this IEnumerable<ioschools.DB.students_guardian> rows, int year)
        {
            foreach (var row in rows)
            {
                yield return new UserChild()
                                 {
                                     linkid = row.id,
                                     id = row.user.id.ToString(),
                                     name = row.user.ToName(),
                                     relationship = row.type.HasValue ? ((GuardianType)row.type.Value).ToString() : "",
                                     class_name =
                                         row.user.classes_students_allocateds.Count(y => y.year == year) !=
                                         0
                                             ? row.user.
                                                   classes_students_allocateds.Single(y => y.year == year).school_class.name
                                             : ""

                                 };
            }
        }
    }
}