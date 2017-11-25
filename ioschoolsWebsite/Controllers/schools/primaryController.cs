using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschoolsWebsite.Controllers.schools
{
    public class primaryController : baseController
    {
        public ActionResult Eca()
        {
            return View(baseviewmodel);
        }

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }
    }
}
