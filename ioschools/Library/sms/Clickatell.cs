using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using clearpixels.Logging;

namespace ioschools.Library.sms
{
    public static class Clickatell
    {
        private const string commandUrl =
            "http://api.clickatell.com/http/sendmsg?user=USERNAME&password=PASSWORD&api_id=API_ID&to={0}&text={1}";

        public static bool Send(string message, string number)
        {
            var requestUrl = string.Format(commandUrl, number.ToNumbersOnly(), message + " : School");
            string content;
            WebResponse response;
            try
            {
                var request = WebRequest.Create(requestUrl);
                request.Method = "GET";
                response = request.GetResponse();
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    content = sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                response = ex.Response;
                if (response != null)
                {
                    using (var sr = new StreamReader(response.GetResponseStream()))
                    {
                        var error = sr.ReadToEnd();
                        Syslog.Write(ErrorLevel.ERROR, "SMS Error: " + requestUrl + " " + error);
                    }
                }
                return false;
            }

            return true;
        }
    }
}