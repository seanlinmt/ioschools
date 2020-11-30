using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Library;
using ioschools.Models.user;

namespace ioschools.Models.email
{
    public class EmailStudentViewModel
    {
        public string receiver { get; set; }
        public string offender { get; set; }
        public string receiverEmail { get; set; }
        public string date { get; set; }
    }

    public static class EmailStudentViewModelHelper
    {
        public static EmailStudentViewModel ToAttendanceEmailModel(this ioschools.DB.user usr)
        {
            var data = new EmailStudentViewModel()
                       {
                           date = Utility.GetDBDate().ToString(Constants.DATEFORMAT_DATEPICKER),
                           offender = usr.ToName()
                       };
            var receiver = usr.students_guardians.Where(x => !string.IsNullOrEmpty(x.user1.email)).Select(
                x => x.user1).FirstOrDefault();
            if (receiver != null)
            {
                data.receiverEmail = receiver.email;
                data.receiver = receiver.ToName();
            }
            return data;
        }
    }
}