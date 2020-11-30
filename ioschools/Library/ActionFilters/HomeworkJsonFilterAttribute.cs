using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using ioschools.Models.homework;
using clearpixels.Logging;

namespace ioschools.Library.ActionFilters
{
    public class HomeworkJsonFilterAttribute : ActionFilterAttribute
    {
        public string Param { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json") &&
                filterContext.HttpContext.Request.HttpMethod == "POST")
            {
                try
                {
                    using (var sr = new StreamReader(filterContext.HttpContext.Request.InputStream))
                    {
                        var inputstring = sr.ReadToEnd();
                        var o = new JavaScriptSerializer().Deserialize<Homework>(inputstring);
                        filterContext.ActionParameters[Param] = o;
                    }
                }
                catch (Exception ex)
                {
                    Syslog.Write(ex);
                    filterContext.HttpContext.Response.Write(ex.Message);
                }
                
            }
        }
    }
}