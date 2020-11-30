using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Library.Helpers;

namespace ioschools.Library.email
{
    public static class EmailHelper
    {
        public static void SendEmail(this Controller controller, EmailViewType type,
                    object viewData, string subject, string destEmail, string destName, string sourceEmail = "", IEnumerable<string> cclist = null, bool queue = true)
        {
            string body = controller.RenderViewToString(type.ToDescriptionString(), viewData);

            // wrap body with div to format it
            body = string.Concat("<div style=\"font: 12px/18px arial, sans-serif;\"", body, "</div>");
            if (!string.IsNullOrEmpty(sourceEmail))
            {
                Email.SendMail("", sourceEmail, destName, destEmail, subject, body, queue, cclist);
            }
            else
            {
                Email.SendMail(destName, destEmail, subject, body, queue, cclist);
            }
        }

        public static void SendEmailNow(this Controller controller, EmailViewType type,
                    object viewData, string subject, string destEmail, string destName, string sourceEmail = "", IEnumerable<string> cclist = null)
        {
            controller.SendEmail(type, viewData, subject, destEmail, destName, sourceEmail, cclist, false);
        }

        /// <summary>
        /// non TModel partial views
        /// </summary>
        /// <param name="type"></param>
        /// <param name="viewData"></param>
        /// <param name="subject"></param>
        /// <param name="destEmail"></param>
        /// <param name="destName"></param>
        /// <param name="ownerid"></param>
        /// <param name="cclist"></param>
        /// <param name="queueMail"></param>
        public static void SendEmail(EmailViewType type,
                    ViewDataDictionary viewData, string subject, string destEmail, string destName, IEnumerable<string> cclist = null, bool queueMail = true)
        {
            string body = ViewHelpers.RenderViewToString(type.ToDescriptionString(), viewData);

            // wrap body with div to format it
            body = string.Concat("<div style=\"font: 12px/18px arial, sans-serif;\"", body, "</div>");
            Email.SendMail(destName, destEmail, subject, body, queueMail, cclist);
        }

        public static void SendEmailNow(EmailViewType type,
                    ViewDataDictionary viewData, string subject, string destEmail, string destName, IEnumerable<string> cclist = null)
        {
            SendEmail(type,viewData,subject,destEmail,destName, cclist, false);
        }

        public static string ToMailToLink(this string email, string subject)
        {
            string emailLink = email;
            if (!string.IsNullOrEmpty(subject))
            {
                emailLink += "?subject=" + subject;
            }
            return string.Concat("<a href=\"mailto:", emailLink, "\" >", email, "</a>");
        }

        public static string ToObfuscatedEmail(this string email)
        {
            email = email.Replace(".", " dot ");
            email = email.Replace("@", " at ");
            return email;
        }
    }
}