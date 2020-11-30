using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Controllers
{
    public class resourcesController : baseController
    {
        //
        // GET: /resources/

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
