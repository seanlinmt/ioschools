using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Controllers.schools
{
    public class internationalController : baseController
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
