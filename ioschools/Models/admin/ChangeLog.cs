using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.DB;
using ioschools.Models.user;

namespace ioschools.Models.admin
{
    public class ChangeLog
    {
        public int id { get; set; }
        public string creatorname { get; set; }
        public long creatorid { get; set; }
        public string changeDate { get; set; }
        public string changes { get; set; }
    }

    public static class ChangeLogHelper
    {
        public static IEnumerable<ChangeLog> ToModel(this IEnumerable<changelog> rows)
        {
            foreach (var row in rows)
            {
                yield return new ChangeLog()
                                 {
                                     changeDate = row.created.ToString(Constants.DATETIME_FULL),
                                     creatorname = row.user.ToName(),
                                     creatorid = row.userid,
                                     id = row.id,
                                     changes = row.changes
                                 };
            }
        }
    }
}