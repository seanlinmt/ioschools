using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Web;
using System.Web.Caching;
using ioschools.Data;
using ioschools.Library.Helpers;
using clearpixels.Logging;

namespace ioschools.Library.scheduler
{
    public sealed class CacheScheduler
    {
#if DEBUG
        public const string HTTP_CACHEURL = "http://localhost:33224/dummy";
#else
        public const string HTTP_CACHEURL = "http://localhost/dummy";
#endif

        private enum TaskType
        {
            Email,
            Registration
        }
        
        public readonly static CacheScheduler Instance = new CacheScheduler();
        private readonly Dictionary<TaskType,Thread> runningThreads = new Dictionary<TaskType, Thread>();
        private CacheScheduler()
        {
            
        }

        public void RegisterCacheEntry()
        {
            Debug.WriteLine("RegisterCacheEntry .....");
            // Prevent duplicate key addition
            if (HttpRuntime.Cache[CacheTimerType.Minute1.ToString()] == null)
            {
                HttpRuntime.Cache.Add(CacheTimerType.Minute1.ToString(), 1, null, DateTime.Now.AddSeconds(60),
                    Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                    CacheItemRemovedCallback);
            }

            if (HttpRuntime.Cache[CacheTimerType.Minute5.ToString()] == null)
            {
                HttpRuntime.Cache.Add(CacheTimerType.Minute5.ToString(), 5, null, DateTime.Now.AddMinutes(5),
                        Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                        CacheItemRemovedCallback);
            }

            if (HttpRuntime.Cache[CacheTimerType.Minute10.ToString()] == null)
            {
                HttpRuntime.Cache.Add(CacheTimerType.Minute10.ToString(), 10, null, DateTime.Now.AddMinutes(10),
                        Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                        CacheItemRemovedCallback);
            }

            if (HttpRuntime.Cache[CacheTimerType.Minute60.ToString()] == null)
            {
                HttpRuntime.Cache.Add(CacheTimerType.Minute60.ToString(), 60, null, DateTime.Now.AddMinutes(60),
                         Cache.NoSlidingExpiration, CacheItemPriority.NotRemovable,
                        CacheItemRemovedCallback);
            }
        }

        private void CacheItemRemovedCallback(
            string key,
            object value,
            CacheItemRemovedReason reason
            )
        {
            //if (reason != CacheItemRemovedReason.Expired)
            //{
            //    eJException newex = new eJException();
            //    newex.logException("cacheExpired: " + key + " " + reason.ToString(), null);
            //}
            Debug.WriteLine("Cache Expired: " + key);
            switch (key.ToEnum<CacheTimerType>())
            {
#if DEBUG
                case CacheTimerType.Minute5:

                    var thread = new Thread(ScheduledTask.SendEmails) {Name = TaskType.Email.ToString()};
                    if (!runningThreads.ContainsKey(TaskType.Email))
                    {
                        runningThreads.Add(TaskType.Email, thread);
                        thread.Start();
                    }
                    else
                    {
                        if (!runningThreads[TaskType.Email].IsAlive)
                        {
                            runningThreads[TaskType.Email] = thread;
                            thread.Start();
                        }
                    }
                    thread = new Thread(ScheduledTask.SetStudentInactiveIfLeavingDateSet) {Name = TaskType.Registration.ToString()};
                    if (!runningThreads.ContainsKey(TaskType.Registration))
                    {
                        runningThreads.Add(TaskType.Registration, thread);
                        thread.Start();
                    }
                    else
                    {
                        if (!runningThreads[TaskType.Registration].IsAlive)
                        {
                            runningThreads[TaskType.Registration] = thread;
                            thread.Start();
                        }
                    }
                    break;
#else
                case CacheTimerType.Minute1:
                    break;
                case CacheTimerType.Minute5:
                    var thread = new Thread(ScheduledTask.SendEmails) { Name = TaskType.Email.ToString() };
                    if (!runningThreads.ContainsKey(TaskType.Email))
                    {
                        runningThreads.Add(TaskType.Email, thread);
                        thread.Start();
                    }
                    else
                    {
                        if (!runningThreads[TaskType.Email].IsAlive)
                        {
                            runningThreads[TaskType.Email] = thread;
                            thread.Start();
                        }
                    }
                    break;
                case CacheTimerType.Minute10:
                    break;
                case CacheTimerType.Minute60:
                    thread = new Thread(ScheduledTask.SetStudentInactiveIfLeavingDateSet) {Name = TaskType.Registration.ToString()};
                    if (!runningThreads.ContainsKey(TaskType.Registration))
                    {
                        runningThreads.Add(TaskType.Registration, thread);
                        thread.Start();
                    }
                    else
                    {
                        if (!runningThreads[TaskType.Registration].IsAlive)
                        {
                            runningThreads[TaskType.Registration] = thread;
                            thread.Start();
                        }
                    }
                    break;
#endif
                default:
#if !DEBUG
                    Syslog.Write(ErrorLevel.CRITICAL, "CacheScheduler ERROR: " + key);
#endif
                    break;
            }
            HitPage();
        }

        private static void HitPage()
        {
            using (var client = new WebClient())
            {
                client.DownloadData(HTTP_CACHEURL);    
            }
        }
    }
}
