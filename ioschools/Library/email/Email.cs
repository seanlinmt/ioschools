using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Text;
using clearpixels.Logging;
using ioschools.DB;
using clearpixels.Email;

namespace ioschools.Library.email
{
    public static class Email
    {
        public const string MAIL_SERVER = "MAIL_SERVER";
        public const string MAIL_SOURCE_ADDRESS = "MAIL_SOURCE_ADDRESS";
        public const string MAIL_PASSWORD = "MAIL_PASSWORD";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="isAsync"></param>
        /// <param name="queueMail"></param>
        /// <param name="ccList"></param>
        public static void SendMail(mail entry, bool isAsync, bool queueMail, IEnumerable<string> ccList = null)
        {
            // need to check for invalid email address
            if (!entry.toEmail.IsEmail())
            {
                return;
            }

            if (queueMail)
            {
                // queue it instead
                using (var db = new ioschoolsDBDataContext())
                {
                    db.mails.InsertOnSubmit(entry);
                    db.SubmitChanges();
                }
                return;
            }

            var from = new MailAddress(MAIL_SOURCE_ADDRESS, " School", Encoding.UTF8);
            MailAddress replyto = null;
            if (!string.IsNullOrEmpty(entry.fromEmail))
            {
                replyto = new MailAddress(entry.fromEmail, entry.fromName, Encoding.UTF8);
            }
            var to = new MailAddress(entry.toEmail, entry.toName, Encoding.UTF8);
            var msg = new MailMessage(from, to)
                                  {
                                      Body = entry.body,
                                      IsBodyHtml = true,
                                      BodyEncoding = Encoding.UTF8,
                                      Subject = entry.subject,
                                      SubjectEncoding = Encoding.UTF8
                                  };
            
            // add footer
            if (replyto != null)
            {
                msg.ReplyToList.Add(replyto);
                msg.Body += "<p>You can directly reply to this email.</p>";
            }
            else
            {
                msg.Body += "<p>This is an automated mail. Please DO NOT reply to this email.</p>";
            }

            // cclist
            if (ccList != null)
            {
                foreach (var email in ccList)
                {
                    msg.CC.Add(new MailAddress(email));
                }
            }
            var smtp = new SmtpClient(MAIL_SERVER)
                           {Credentials = new NetworkCredential(MAIL_SOURCE_ADDRESS, MAIL_PASSWORD)};
            if (isAsync)
            {
                smtp.SendCompleted += SendCompletedCallback;
                smtp.SendAsync(msg, entry);
            }
            else
            {
                try
                {
                    smtp.Send(msg);
                }
                catch(SmtpFailedRecipientException ex)
                {
                    Syslog.Write(ex);
                }
                catch (Exception ex)
                {
                    Syslog.Write(ex);
                    // then we need to reinsert back
                    // reinsert back into database
                    using (var db = new ioschoolsDBDataContext())
                    {
                        db.mails.InsertOnSubmit(entry);
                        db.SubmitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toName"></param>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="owner"></param>
        /// <param name="queueMail"> </param>
        /// <param name="cclist"> </param>
        public static void SendMail(string toName, string toEmail, string subject, string body, bool queueMail, IEnumerable<string> cclist)
        {
            if (toName == null)
            {
                toName = "";
            }
            var entry = new mail
                            {
                                toName = toName,
                                toEmail = toEmail,
                                subject = subject,
                                body = body
                            };
            SendMail(entry, true, queueMail, cclist);
        }

        public static void SendMail(string fromName, string fromEmail, string toName, string toEmail, 
            string subject, string body, bool queueMail, IEnumerable<string> cclist)
        {
            if (toName == null)
            {
                toName = "";
            }
            if (fromName == null)
            {
                fromName = "";
            }
            var entry = new mail
            {
                fromName = fromName,
                fromEmail = fromEmail,
                toName = toName,
                toEmail = toEmail,
                subject = subject,
                body = body
            };
            SendMail(entry, true, queueMail, cclist);
        }

        private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            // get the identifier
            var token = (mail)e.UserState;
            if (e.Error != null)
            {
                Syslog.Write(e.Error);
                if (e.Error.GetType() != typeof(SmtpFailedRecipientException))
                {
                    // reinsert back into database
                    using (var db = new ioschoolsDBDataContext())
                    {
                        db.mails.InsertOnSubmit(token);
                        db.SubmitChanges();
                    }
                }
            }

            // update order status
            //ITradelrRepository repository = new TradelrRepository();
            //repository.UpdateOrderStatus(token.orderID, token.userID, OrderStatus.SENT);

        }
    }

    
}
