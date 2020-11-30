using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Web.Mvc;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.sms;
using ioschools.Models.exam;
using ioschools.Models.user;
using NPOI.HSSF.UserModel;
using ioschools.DB;

namespace ioschools.Controllers
{
    [ElmahError]
    public class testController : baseController
    {
#if TEST
        //
        // GET: /test/
        public ActionResult list()
        {
            var emails = new List<string>();
            emails.Add("a");
            if (!emails.Contains("a"))
            {
                return Content(true.ToString());
            }

            return Content(false.ToString());
        }

        public ActionResult sendmail()
        {
            new Thread(() =>
            {
                var viewdata = new ViewDataDictionary
                                       {
                                           {"title", "test"},
                                           {"content", "test"},
                                           {"sender", "sean lin"}
                                       };
                EmailHelper.SendEmailNow(
                                EmailViewType.CIRCULAR,
                                viewdata,
                                "test",
                                "seanlinmt@gmail.com",
                                "sean lin");
            }).Start();
            return Content("done");
        }

        public ActionResult dob()
        {
            var date1 = "760526888888".ToDOB();
            var date2 = "990526888888".ToDOB();
            var date3 = "120111888888".ToDOB();

            return Content(string.Format("{0} {1} {2}", date1, date2, date3));
        }

        public ActionResult compare()
        {
            return Content(((double)5/3).ToString("n2") + " " + Math.Round((double)5/3,2));
        }

        public ActionResult redblack()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 512; i++)
            {
                string binary = int.Parse(Convert.ToString(i, 2)).ToString("D9");
                sb.Append(binary + "<br/>");
            }
            return Content(sb.ToString());
        }

        public ActionResult position(long id)
        {
            var calc = new PositionCalculator();
            calc.CalculateAndSaveSubjectPosition(id);

            var sb = new StringBuilder();

            return Content("done");
        }

        public ActionResult report()
        {
            var ms = new MemoryStream();
            using (FileStream fs =
                new FileStream(
                    AppDomain.CurrentDomain.BaseDirectory + "/Content/templates/ReportCardSecondary.xls",
                    FileMode.Open, FileAccess.Read))
            {
                HSSFWorkbook templateWorkbook = new HSSFWorkbook(fs, true);

                // need to find first term results and / or second term results
            }
            return File(ms.ToArray(), "application/vnd.ms-excel", string.Format("ReportCard_{0}.xls", DateTime.Now.ToShortDateString().Replace("/", "")));
        }

        public ActionResult TestSMS()
        {
            Clickatell.Send("yippeeeeee", "0165760616");

            return Content("done");
        }
#endif
    }
}
