using ioschools.Data.Attendance;
using ioschools.Data.User;

namespace ioschools.Models.user
{
    // !!!! field names are tied to jqgrid_users.js
    public class UserSearch
    {
        public int? school { get; set; }
        public int? sclass { get; set; }
        public AttendanceStatus? attStatus { get; set; }
        public string date { get; set; }
        public UserGroup? group { get; set; }
        public string discipline { get; set; }
        public bool? status { get; set; }
        public string term { get; set; }
        public int year { get; set; }
        public int? seca { get; set; }
        public bool hasIssues { get; set; }
    }
}