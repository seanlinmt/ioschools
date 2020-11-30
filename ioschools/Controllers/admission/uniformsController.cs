using System.Web.Mvc;

namespace ioschools.Controllers.admission
{
    public class uniformsController : baseController
    {
        //
        // GET: /uniforms/

        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
