using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Library;

namespace ioschools.Controllers.schools
{
    public class kindergartenController : baseController
    {
        public ActionResult AfterSchool()
        {
            return View(baseviewmodel);
        }

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

        public ActionResult Classes()
        {
            var classes = repository.GetSchoolClasses().Where(x => x.schoolid == (int) Schools.Kindergarten).Select(x => new{ x.id, x.name});
            return Json(classes.ToJsonOKData());
        }
    }
}
