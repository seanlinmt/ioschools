using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.Models.user;
using clearpixels.Logging;
using clearpixels.crypto.token;

namespace ioschools.Library.ActionFilters
{
    public class GroupsFilterAttribute : ActionFilterAttribute
    {
        public UserGroup group { get; set; }

        private void LogAccessError(ActionExecutingContext filterContext, string userid)
        {
            Syslog.Write(ErrorLevel.WARNING, string.Format("Group Filter: Attempted access by {0} for {1}/{2}",
                        userid,
                        filterContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                        filterContext.ActionDescriptor.ActionName));
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var token = (BasicSecurityToken)filterContext.HttpContext.Items["token"];
            if (token != null)
            {
                var userperm = token.Group.ToEnum<UserGroup>();
                if (((group & userperm) == 0) && userperm != UserGroup.ADMIN)
                {
                    if (filterContext.IsChildAction)
                    {
                        // don't set status here as it seems to cause IIS to return error page
                        filterContext.Result = new EmptyResult();
                    }
                    else if(filterContext.HttpContext.Request.IsAjaxRequest())
                    {
                        filterContext.RequestContext.HttpContext.Response.StatusCode = HttpStatusCode.Forbidden.ToInt();
                        filterContext.Result = new EmptyResult();

                        LogAccessError(filterContext, token.UserID);
                    }
                    else
                    {
                        filterContext.RequestContext.HttpContext.Response.StatusCode = HttpStatusCode.Forbidden.ToInt();
                        filterContext.Result = new ViewResult() { ViewName = "~/Views/Error/NoPermission.aspx" };

                        LogAccessError(filterContext, token.UserID);
                    }
                    return;
                }
            }
            else
            {
                // then user is not logged in 
                filterContext.RequestContext.HttpContext.Response.StatusCode = HttpStatusCode.Unauthorized.ToInt();
                if (filterContext.HttpContext.Request.IsAjaxRequest() || filterContext.IsChildAction)
                {
                    filterContext.Result = new EmptyResult();
                }
                else
                {
                    filterContext.Result = new RedirectResult("/login?redirect=" + HttpUtility.UrlEncode(filterContext.HttpContext.Request.Url.PathAndQuery));
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}