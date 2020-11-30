using System;
using System.Web.Mvc;
using Elmah;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.DB;
using ioschools.DB.repository;
using ioschools.Library;
using ioschools.Models;
using clearpixels.crypto.token;

namespace ioschools.Controllers
{
    public class baseController : Controller
    {
        private const int COOKIE_LIFETIME = 1209600; // 14 days 
#if DEBUG
        private const int COOKIE_LIFETIME_MIN = 300; // 5 minutes 
#else
        private const int COOKIE_LIFETIME_MIN = 3600; // 60 mins 
#endif
        protected readonly BaseViewModel baseviewmodel;
        protected readonly IRepository repository;
        protected readonly ioschoolsDBDataContext db;

        protected UserAuth auth;
        protected long? sessionid;
        private BasicSecurityToken token;

        public baseController()
        {
            baseviewmodel = new BaseViewModel();
            db = new ioschoolsDBDataContext();
            repository = new Repository(db);
            auth = new UserAuth();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            /*
            // check url referrer to prevent CSRF attacks
            if (Request.UrlReferrer != null)
            {
#if DEBUG
                if (!Request.UrlReferrer.Host.Contains("localhost"))
#else
                    if (!Request.UrlReferrer.Host.Contains("lodgeschool"))
#endif
                {
                    filterContext.Result = new RedirectResult("/Error/NoPermission");
                    return;
                }
            }
             * */

            token = Request.RequestContext.HttpContext.Items["token"] as BasicSecurityToken;

            if (token == null)
            {
                GetAuthCookie();
            }

            if (token != null)
            {
                Request.RequestContext.HttpContext.Items["token"] = token;
                baseviewmodel.isLoggedIn = true;
                baseviewmodel.sessionid = long.Parse(token.UserID);
                sessionid = baseviewmodel.sessionid;
                baseviewmodel.userauth.group = token.Group.ToEnum<UserGroup>();
                baseviewmodel.userauth.perms = token.Permission.ToEnum<Permission>();
                auth = baseviewmodel.userauth;
                baseviewmodel.name = token.UserName;
            }
            
            base.OnActionExecuting(filterContext);
        }

        private void GetAuthCookie()
        {
            if (Request.Cookies != null)
            {
                var httpCookie = Request.Cookies["token"];
                if (httpCookie != null)
                {
                    try
                    {
                        token = new BasicSecurityToken(httpCookie.Value);
                    }
                    catch (Exception ex)
                    {
                        // expired, clear cookie
                        ClearAuthCookie();
                        ClearOldCookie();
                    }
                }
            }
        }

        protected void SetAuthCookie(user usr, bool rememberme)
        {
            DateTime expires;
            if (rememberme)
            {
                expires = DateTime.UtcNow.AddSeconds(COOKIE_LIFETIME);
            }
            else
            {
                expires = DateTime.UtcNow.AddSeconds(COOKIE_LIFETIME_MIN);
            }

            token = new BasicSecurityToken(usr.id, usr.ToName(), usr.usergroup, usr.permissions, 0, "", expires);
            Response.Cookies["token"].Value = token.Serialize();
            Response.Cookies["token"].Expires = expires;
        }

        protected void ClearAuthCookie()
        {
            var httpCookie = Response.Cookies["token"];
            if (httpCookie != null) httpCookie.Expires = DateTime.Now.AddMonths(-1);
        }

        protected void ClearOldCookie()
        {
            var httpCookie = Response.Cookies["auth"];
            if (httpCookie != null) httpCookie.Expires = DateTime.Now.AddMonths(-1);
        }

        protected ActionResult ReturnNotFoundView()
        {
            return View("~/Views/Error/NotFound.cshtml");
        }

        protected ActionResult ReturnNoPermissionView()
        {
            return View("~/Views/Error/NoPermission.cshtml");
        }

        protected ActionResult SendJsonErrorResponse(Exception ex)
        {
            ErrorSignal.FromCurrentContext().Raise(ex);
            return Json("Oops, we did something wrong. This will be fixed within 24 hours.".ToJsonFail(), JsonRequestBehavior.AllowGet);
        }

        protected ActionResult SendJsonErrorResponse(string message)
        {
            ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("{0}: {1}", sessionid, message)));
            return Json(message.ToJsonFail(), JsonRequestBehavior.AllowGet);
        }

        protected ActionResult SendJsonNoPermission(string message = "You are not allowed to do that")
        {
            ErrorSignal.FromCurrentContext().Raise(new Exception(string.Format("{0}: {1}", sessionid, message)));
            return Json(message.ToJsonFail(), JsonRequestBehavior.AllowGet);
        }
    }
}
