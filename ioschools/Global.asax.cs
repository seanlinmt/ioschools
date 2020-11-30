using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ioschools.Data;
using ioschools.Controllers.error;
using ioschools.Library.Lucene;
using ioschools.Library.scheduler;
using clearpixels.Logging;

namespace ioschools
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        private const string LUCENE_THREAD_NAME = "LuceneThread";
        private readonly Dictionary<string, Thread> runningThreads = new Dictionary<string, Thread>();

        private static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon(\.ico)?" });

            routes.MapRoute(
                "Javascripts", // Route name
                "js/{action}.js", // URL with parameters
                new { controller = "js" }
            );

            routes.MapRoute(
                "CSS", // Route name
                "css/{action}.css", // URL with parameters
                new { controller = "css" }
            );

            routes.MapRoute(
                "News", // Route name
                "news/{id}/{title}", // URL with parameters
                new { controller = "News", action = "Single", title = "" },
                new { id = @"\d+" } 
            );

            routes.MapRoute(
                "PhotoUpload", // Route name
                "photo/upload/{type}/{id}", // URL with parameters
                new { controller = "Photo", action = "Upload", type = "none", id = "" } // Parameter defaults
            );

            routes.MapRoute(
                "Single User View",                                              // Route name
                "users/{id}",                           // URL with parameters
                new { controller = "Users", action = "Single" },
                new { id = @"\d+" }
            );


            routes.MapRoute(
                "Index View",                                              // Route name
                "{controller}/{id}",                           // URL with parameters
                new { action = "Index" },
                new { id = @"\d+" }
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_End()
        {
            //SqlDependency.Stop(ConfigurationManager.ConnectionStrings["ioschoolsConnectionString"].ConnectionString);
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterRoutes(RouteTable.Routes);

            MvcHandler.DisableMvcResponseHeader = true;
            // start cache-based scheduler
            CacheScheduler.Instance.RegisterCacheEntry();
            //SqlDependency.Start(ConfigurationManager.ConnectionStrings["ioschoolsConnectionString"].ConnectionString);
            var luceneThreadRunning = false;
            Thread thread = null;
            if (runningThreads.TryGetValue(LUCENE_THREAD_NAME, out thread))
            {
                if (thread.IsAlive)
                {
                    luceneThreadRunning = true;
                    Syslog.Write(ErrorLevel.INFORMATION, "Lucene Index already building");
                }
            }
            if (!luceneThreadRunning)
            {
                var lucenethread = new Thread(LuceneUtil.ThreadProcBuild) {Name = LUCENE_THREAD_NAME};
                lucenethread.Start();
                runningThreads[LUCENE_THREAD_NAME] = lucenethread;
                Syslog.Write(ErrorLevel.INFORMATION, "Building Lucene Index");
            }
        }

        protected void Application_BeginRequest()
        {
            if (HttpContext.Current.Request.Url.ToString() == CacheScheduler.HTTP_CACHEURL)
            {
                // Add the item in cache and when succesful, do the work.
                CacheScheduler.Instance.RegisterCacheEntry();
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var app = (MvcApplication)sender;
            var context = app.Context;
            var ex = app.Server.GetLastError();
            context.Response.Clear();
            context.ClearError();
            var httpException = ex as HttpException;

            var routeData = new RouteData();
            routeData.Values["controller"] = "error";
            routeData.Values["action"] = "Index";
            if (httpException != null)
            {
                switch (httpException.GetHttpCode())
                {
                    case 403:
                        routeData.Values["action"] = "NoPermission";
                        break;
                    case 404:
                        routeData.Values["action"] = "NotFound";
                        break;
                    default:
                        routeData.Values["action"] = "Index";
                        break;
                }
            }
            IController controller = new errorController();
            controller.Execute(new RequestContext(new HttpContextWrapper(context), routeData));
        }

        protected void Session_Start()
        {
            
        }

        protected void Session_End()
        {
            
        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            if (custom.ToLower() == "user")
            {
                var sessionid = context.Request.Cookies["ASP.NET_SessionId"];
                if (sessionid != null)
                {
                    return sessionid.Value;
                }
            }
            
            return base.GetVaryByCustomString(context, custom);
        }
    }
}