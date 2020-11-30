using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Controllers
{
    public class contactController : baseController
    {
        //
        // GET: /contact/

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
