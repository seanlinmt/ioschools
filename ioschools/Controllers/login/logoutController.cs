using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Controllers.login
{
    public class logoutController : baseController
    {
        public ActionResult Index()
        {
            ClearAuthCookie();
            return Redirect("/login");
        }

    }
}
