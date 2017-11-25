using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschoolsWebsite.Library.ActionFilters;

namespace ioschoolsWebsite.Controllers
{
    public class helpController : baseController
    {
        [PermissionFilter(perm = Permission.NONE)]
        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
