using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Library.Helpers;
using ioschools.Models.user;
using ioschools.DB;

namespace ioschools.Models.fees
{
    public class SchoolFeeUpdateRow
    {
        public string id { get; set; }
        public long studentid { get; set; }
        public string studentname { get; set; }
        public string classname { get; set; }
        public IEnumerable<SelectListItem> statusList { get; set; }
        public string amount { get; set; }
    }


    public static class SchoolFeeAdminHelper
    {
        public static IEnumerable<SchoolFeeUpdateRow> ToModel(this IEnumerable<fee> rows, int year)
        {
            foreach (var row in rows)
            {
                yield return new SchoolFeeUpdateRow()
                                 {
                                     id = row.id.ToString(),
                                     studentid = row.studentid,
                                     studentname = row.user.ToName(false),
                                     classname = row.user.classes_students_allocateds.First(x => x.year == year).school_class.name,
                                     amount = row.amount.ToString("n2"),
                                     statusList = typeof(FeePaymentStatus).ToSelectList(false,null,null,row.status)
                                 };
            }
        }

        public static IEnumerable<SchoolFeeUpdateRow> ToModel(this IQueryable<ioschools.DB.user> rows, int year, decimal amount)
        {
            foreach (var row in rows)
            {
                yield return new SchoolFeeUpdateRow()
                {
                    amount = amount.ToString("n2"),
                    studentid = row.id,
                    studentname = row.ToName(false),
                    classname = row.classes_students_allocateds.First(x => x.year == year).school_class.name,
                    statusList = typeof(FeePaymentStatus).ToSelectList(false, null, null)
                };
            }
        }
    }
}