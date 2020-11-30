using System;
using System.IO;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using clearpixels.Logging;

namespace ioschools.Library.ActionFilters
{
    public class JsonFilterAttribute : ActionFilterAttribute
    {
        public string Param { get; set; }
        public Type RootType { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if ((filterContext.HttpContext.Request.ContentType ?? string.Empty).Contains("application/json") &&
                filterContext.HttpContext.Request.HttpMethod == "POST")
            {
                var inputstring = "";
                try
                {
                    filterContext.HttpContext.Request.InputStream.Position = 0;
                    using (var sr = new StreamReader(filterContext.HttpContext.Request.InputStream))
                    {
                        inputstring = sr.ReadToEnd();
                        var o = new JavaScriptSerializer().Deserialize(inputstring, RootType);
                        filterContext.ActionParameters[Param] = o;
                    }
                }
                catch (Exception ex)
                {
                    Syslog.Write(ErrorLevel.ERROR, string.Format("JsonFilterAttribute: {0} {1}", RootType, inputstring));
                    filterContext.HttpContext.Response.Write(ex.Message);
                }
                
            }
        }
    }
}