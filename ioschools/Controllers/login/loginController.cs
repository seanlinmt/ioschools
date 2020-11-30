using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Models.user;

namespace ioschools.Controllers.login
{
    public class loginController : baseController
    {
        [HttpGet]
        public ActionResult Index(string redirect)
        {
            if (sessionid.HasValue)
            {
                if (!string.IsNullOrEmpty(redirect))
                {
                    return Redirect(redirect);
                }

                return Redirect("/dashboard");
            }

            return View();
        }

        [HttpPost]
        public ActionResult Index(string email, string password, bool remember_me, string redirect)
        {
            // only trim email, password not trimmed because we would need to trim when it is set as well
            if (string.IsNullOrEmpty(email))
            {
                return Json("Please specify an email address".ToJsonFail());
            }

            email = email.Trim().ToLower();
            password = password.Trim();

            if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password))
            {
                var usr = repository.GetActiveUserLogin(email, password);

                if (usr != null)
                {
                    // set auth info
                    SetAuthCookie(usr, remember_me);
                }
                else // login failed
                {
                    // check if account is disabled
                    if (repository.GetUsers().SingleOrDefault(x => x.email == email && (x.settings & (int)UserSettings.INACTIVE) != 0) != null)
                    {
                        return Json("Your account is currently inactive".ToJsonFail());
                    }

                    Syslog.Write(ErrorLevel.WARNING, "Login failed " + email);
                    return Json("The email or password that you entered is incorrect".ToJsonFail());
                }

                //Syslog.Write(ErrorLevel.INFORMATION, "Login successful " + email);

                // create host name
                string homeUrl;
                if (!string.IsNullOrEmpty(redirect))
                {
                    homeUrl = HttpUtility.UrlDecode(redirect);
                }
                else
                {
                    homeUrl = "/dashboard";
                }
                return Json(homeUrl.ToJsonOKData());
            }
            return Json("Some fields are missing".ToJsonFail());
        }

        [HttpPost]
        public ActionResult Forgot(string email)
        {
            email = email.Trim().ToLower();
            var usr = repository.GetActiveUsers().SingleOrDefault(x => string.Compare(x.email, email) == 0);
            if (usr == null)
            {
                return SendJsonErrorResponse("Email address is invalid or does not exist: " + email);
            }

            var password = clearpixels.crypto.Utility.GetRandomString(uppercase: true);

            // save password hash
            var hash = Utility.GeneratePasswordHash(email, password);
            usr.passwordhash = hash;

            // set flag
            usr.settings = usr.SetFlag(UserSettings.PASSWORD_RESET);
            repository.Save();

            // log this just to see how many use this feature
            Syslog.Write(ErrorLevel.INFORMATION, email +  " forgot password");

            // email new password to user
            var credentials = new UserCredentials {password = password, email = email};
            this.SendEmailNow(EmailViewType.PASSWORD_RESET, credentials, "Password Reset", email, usr.ToName());

            return Json("Your new password has been sent to the email account you provided".ToJsonOKMessage());
        }

        [HttpGet]
        public ActionResult Password()
        {
            if (!sessionid.HasValue)
            {
                return Json("Permission denied".ToJsonFail(), JsonRequestBehavior.AllowGet);
            }
            return View();
        }

        [HttpPost]
        public ActionResult Password(string newPass, string newPassConfirm)
        {
            newPass = newPass.Trim();
            newPassConfirm = newPassConfirm.Trim();

            if (sessionid == null)
            {
                return Json("Permission denied".ToJsonFail());
            }

            var usr = repository.GetUser(sessionid.Value);
            if (usr == null)
            {
                return SendJsonErrorResponse("Could not locate your account");
            }
            // check both passwords are similar
            if (newPass != newPassConfirm)
            {
                return Json("Passwords are not the same".ToJsonFail());
            }

            usr.passwordhash = Utility.GeneratePasswordHash(usr.email.ToLower(), newPass);

            // unset flag
            usr.settings = usr.UnsetFlag(UserSettings.PASSWORD_RESET);
            repository.Save();

            return Json("Your password has been changed successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Reset(long id)
        {
            var usr = repository.GetUser(id);
            if (usr == null)
            {
                return Json("User not found".ToJsonFail());
            }

            var canedit = usr.GetCanEdit(sessionid.Value, auth);
            if (!canedit)
            {
                return SendJsonNoPermission();
            }

            if (string.IsNullOrEmpty(usr.email))
            {
                return Json("User does not have an email address".ToJsonFail());
            }

            var email = usr.email.ToLower();

            var password = clearpixels.crypto.Utility.GetRandomString(uppercase: true);

            // save password hash
            var hash = Utility.GeneratePasswordHash(email, password);
            usr.passwordhash = hash;

            // set flag
            usr.settings = usr.SetFlag(UserSettings.PASSWORD_RESET);
            repository.Save();

            // log this just to see how many use this feature
            Syslog.Write(ErrorLevel.INFORMATION, email + " reset password");

            // email new password to user
            var credentials = new UserCredentials { password = password, email = email };
            this.SendEmailNow(EmailViewType.PASSWORD_RESET, credentials, "Password Reset", email, usr.ToName());

            return Json("New password emailed successfully".ToJsonOKMessage());
        }
    }
}
