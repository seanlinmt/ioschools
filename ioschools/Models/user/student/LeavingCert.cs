using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.DB;

namespace ioschools.Models.user.student
{
    public class LeavingCert
    {
        public string student_name { get; set; }
        public string reason { get; set; }
        public string remarks { get; set; }
        public long studentid { get; set; }
        public string admissionDate { get; set; }
        public string leavingDate { get; set; }
    }

    public static class LeavingCertHelper
    {
        public static LeavingCert ToCertModel(this registration row)
        {
            return new LeavingCert()
                       {
                           reason = row.cert_reason,
                           remarks = row.cert_remarks,
                           student_name = row.user.ToName(),
                           studentid = row.studentid,
                           admissionDate = row.admissionDate.HasValue ? row.admissionDate.Value.ToString(Constants.DATEFORMAT_DATEPICKER) : "",
                           leavingDate = row.leftDate.HasValue ? row.leftDate.Value.ToString(Constants.DATEFORMAT_DATEPICKER) : ""
                       };
        }
    }
}