using System;
using System.Diagnostics;
using System.Linq;
using ioschools.Data.User;
using ioschools.Models.user;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.DB.repository;
using ioschools.Library.email;

namespace ioschools.Library.scheduler
{
    public static class ScheduledTask
    {
        public static void SendEmails()
        {
            var myLock = new object();
            lock (myLock)
            {
                using (var repository = new Repository())
                {
                    bool haveChanges = false;
#if DEBUG
                    var mails = repository.GetMails().Take(3);
#else
                    var mails = repository.GetMails().Take(50);
#endif
                    foreach (var mail in mails.ToArray())
                    {
                        Email.SendMail(mail, true, false);
                        repository.DeleteMail(mail);
                        haveChanges = true;
                    }
                    if (haveChanges)
                    {
                        repository.Save();
                    }
                }
            }
        }

        public static void SetStudentInactiveIfLeavingDateSet()
        {
            var date = DateTime.Now;
            var myLock = new object();
            lock (myLock)
            {
                using (var db = new ioschoolsDBDataContext())
                {
                    bool hasChange = false;

                    // check for expired students
                    var groups = db.registrations.GroupBy(x => x.user);
                    foreach (var g in groups)
                    {
                        var activeRow = g.Where(x => x.admissionDate.HasValue)
                                        .OrderBy(x => x.admissionDate).LastOrDefault();
                        if (activeRow != null)
                        {
                            var currentStatus = (UserSettings)activeRow.user.settings;
                            if (activeRow.leftDate.HasValue)
                            {
                                if (date > activeRow.leftDate.Value && !currentStatus.HasFlag(UserSettings.INACTIVE))
                                {
                                    activeRow.user.settings = activeRow.user.SetFlag(UserSettings.INACTIVE);
                                    hasChange = true;
                                }
                                
                            }
                            else
                            {
                                if (date > activeRow.admissionDate.Value && currentStatus.HasFlag(UserSettings.INACTIVE))
                                {
                                    activeRow.user.settings = activeRow.user.UnsetFlag(UserSettings.INACTIVE);
                                    hasChange = true;
                                }
                            }
                        }
                        
                    }

                    // check for expired staff
                    var staffgroups = db.employments.GroupBy(x => x.user);
                    foreach (var g in staffgroups)
                    {
                        var activeRow = g.Where(x => x.start_date.HasValue)
                                        .OrderBy(x => x.start_date).LastOrDefault();
                        if (activeRow != null)
                        {
                            var currentStatus = (UserSettings)activeRow.user.settings;
                            if (activeRow.end_date.HasValue)
                            {
                                if (date > activeRow.end_date.Value && !currentStatus.HasFlag(UserSettings.INACTIVE))
                                {
                                    activeRow.user.settings = activeRow.user.SetFlag(UserSettings.INACTIVE);
                                    hasChange = true;
                                }

                            }
                            else
                            {
                                if (date > activeRow.start_date.Value && currentStatus.HasFlag(UserSettings.INACTIVE))
                                {
                                    activeRow.user.settings = activeRow.user.UnsetFlag(UserSettings.INACTIVE);
                                    hasChange = true;
                                }
                            }
                        }

                    }

                    if (hasChange)
                    {
                        try
                        {
                            db.SubmitChanges();
                        }
                        catch (Exception ex)
                        {
                            Syslog.Write(ex);
                        }
                    }
                }
            }
        }

    }
}
