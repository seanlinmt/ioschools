using System.Linq;
using System.Web.Mvc;
using ioschools.Data.CMS;
using ioschools.Library.Imaging;
using ioschools.Models.blog;
using ioschools.Models.calendar;
using ioschools.Models.photo;

namespace ioschools.Controllers.blog
{
    public class newsController : baseController
    {
        private const int NEWS_COUNT = 6;

        public ActionResult Index(int? page)
        {
            var calendar = new Calendar();
            var entries = calendar.GetFutureEvents(10);
            var viewmodel = new BlogSummaryViewData(baseviewmodel)
                                {
                                    events = entries
                                };

            if (!page.HasValue || page.Value < 0)
            {
                page = 0;
            }
            var skipcount = page.Value * NEWS_COUNT;
            var news = repository.GetBlogs().Where(x => x.ispublic);
            var total = news.Count();

            var newspanel = new NewsPanelViewModel();
            newspanel.page = page.Value;
            newspanel.news = news.OrderByDescending(x => x.created).Skip(skipcount).Take(NEWS_COUNT).ToModel();
            newspanel.hasNewer = page != 0;
            if (newspanel.news.Count() + skipcount < total)
            {
                newspanel.hasOlder = true;
            }
            viewmodel.newspanel = newspanel;
            
            return View(viewmodel);
        }

        [HttpGet]
        public ActionResult Single(long id)
        {
            var blog = repository.GetBlog(id);
            if (blog == null)
            {
                return ReturnNotFoundView();
            }

            var viewdata = new BlogViewData(baseviewmodel)
            {
                blog = blog.ToModel(),
                method = blog.layoutMethod.HasValue?(LayoutMethod)blog.layoutMethod.Value:LayoutMethod.NONE,
                attachments = blog.blog_files.ToModel()
            };

            if (viewdata.method == LayoutMethod.GALLERY)
            {
                viewdata.photos = blog.blog_images.ToModel(Imgsize.GALLERY, Imgsize.BLOG);
            }
            else
            {
                viewdata.photos = blog.blog_images.ToModel(Imgsize.USER_PROFILE);
            }

            return View(viewdata);
        }
    }
}
