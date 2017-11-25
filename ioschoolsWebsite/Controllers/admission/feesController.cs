using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ioschoolsWebsite.Controllers.admission
{
    public class feesController : baseController
    {
        //
        // GET: /fees/

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
