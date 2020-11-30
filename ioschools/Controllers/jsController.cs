using System;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.user;

namespace ioschools.Controllers
{
    // compression handled by IIS, does not compress on dev server
    [ElmahError]
    public class jsController : Controller
    {
        public ActionResult core()
        {
            LoadedContent jscontent;
            if (HttpContext.Cache[Request.Path] == null)
            {
                jscontent = JsLoader.Instance.LoadFeatures("core", "files.xml");
                jscontent.content = string.Concat(jscontent.content, GenerateJavascript());
                HttpContext.Cache.Insert(Request.Path, jscontent, new CacheDependency(jscontent.filenames.ToArray()));
            }
            else
            {
                jscontent = (LoadedContent)HttpContext.Cache[Request.Path];
            }

            Response.AddFileDependencies(jscontent.filenames.ToArray());
            Response.Cache.SetLastModifiedFromFileDependencies();
            Response.Cache.SetETagFromFileDependencies();
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetExpires(DateTime.Now.AddHours(1));
            return Content(jscontent.content, "application/x-javascript");
        }

        public ActionResult extend()
        {
            LoadedContent jscontent;
            if (HttpContext.Cache[Request.Path] == null)
            {
                jscontent = JsLoader.Instance.LoadFeatures("extend", "files.xml");
                HttpContext.Cache.Insert(Request.Path, jscontent, new CacheDependency(jscontent.filenames.ToArray()));
            }
            else
            {
                jscontent = (LoadedContent)HttpContext.Cache[Request.Path];
            }

            Response.AddFileDependencies(jscontent.filenames.ToArray());
            Response.Cache.SetLastModifiedFromFileDependencies();
            Response.Cache.SetETagFromFileDependencies();
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetExpires(DateTime.Now.AddHours(1));
            return Content(jscontent.content, "application/x-javascript");
        }

        public ActionResult jqgrid()
        {
            LoadedContent jscontent;
            if (HttpContext.Cache[Request.Path] == null)
            {
                jscontent = JsLoader.Instance.LoadFeatures("jqgrid", "files.xml");
                HttpContext.Cache.Insert(Request.Path, jscontent, new CacheDependency(jscontent.filenames.ToArray()));
            }
            else
            {
                jscontent = (LoadedContent)HttpContext.Cache[Request.Path];
            }

            Response.AddFileDependencies(jscontent.filenames.ToArray());
            Response.Cache.SetLastModifiedFromFileDependencies();
            Response.Cache.SetETagFromFileDependencies();
            Response.Cache.SetCacheability(HttpCacheability.Public);
            Response.Cache.SetExpires(DateTime.Now.AddHours(1));
            return Content(jscontent.content, "application/x-javascript");
        }

        private static string GenerateJavascript()
        {
            var sb = new StringBuilder();
#if DEBUG
            sb.Append("var DEBUG = true;");
#else
            sb.Append("var DEBUG = false;");
#endif

            return sb.ToString();
        }
    }
}
