using System.Web;
using System.Web.Mvc;

namespace ioschools.Library.ActionFilters
{
    public class NoCache : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpResponseBase response = filterContext.HttpContext.Response;
            response.AddHeader("Cache-Control", "no-cache;no-store");
            response.AddHeader("Pragma", "no-cache");
            response.AddHeader("Expires", "-1");
        }
    }
}