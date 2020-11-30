using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.Leave;
using ioschools.DB;

namespace ioschools.Models.leave
{
    public class StaffLeave
    {
        public string id { get; set; }
        public string name { get; set; }
        public IEnumerable<SelectListItem> typeList { get; set; }
        public int? annualTotal { get; set; }
        public decimal? remaining { get; set; }
        public decimal? taken { get; set; }
    }

    public static class StaffLeaveHelper
    {
        public static IEnumerable<StaffLeave> ToModel(this IEnumerable<leaves_allocated> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToModel();
            }
        }

        public static StaffLeave ToModel(this leaves_allocated row)
        {
            return new StaffLeave()
            {
                id = row.id.ToString(),
                name = row.leave.name,
                annualTotal = row.annualTotal,
                remaining = row.remaining,
                taken = row.leaves_takens.Where(x => x.status == (byte)LeaveStatus.APPROVED).Sum(x => x.days)
            };
        }
    }
}