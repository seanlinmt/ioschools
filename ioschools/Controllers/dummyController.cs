using System.Web.Mvc;

namespace ioschools.Controllers
{
    public class dummyController : Controller
    {
        //
        // GET: /dummy/

        public ActionResult Index()
        {
            return new EmptyResult();
        }

    }
}
