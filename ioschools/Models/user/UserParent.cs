using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;

namespace ioschools.Models.user
{
    public class UserParent : IdName
    {
        public long linkid { get; set; } // student_guardian id
        public string contactInfo { get; set; }
        public string relationship { get; set; }
    }

    public static class UserParentHelper
    {
        public static ioschools.DB.user ToParent(this ioschools.DB.user row, GuardianType type)
        {
            return row.students_guardians.Where(
                x => x.type == (int)type).Select(x => x.user1)
                .FirstOrDefault();
        }
    }
}