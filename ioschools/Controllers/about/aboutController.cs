using System.Web.Mvc;

namespace ioschools.Controllers.about
{
    public class aboutController : baseController
    {
        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

        public ActionResult History()
        {
            return View(baseviewmodel);
        }

        public ActionResult Vision()
        {
            return View(baseviewmodel);
        }

        public ActionResult Mission()
        {
            return View(baseviewmodel);
        }

        public ActionResult People()
        {
            return View(baseviewmodel);
        }

        public ActionResult Philosophy()
        {
            return View(baseviewmodel);
        }

        public ActionResult Spirit()
        {
            return View(baseviewmodel);
        }
    }
}
