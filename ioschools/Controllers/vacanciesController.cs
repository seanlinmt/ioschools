using System;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Models.pages;

namespace ioschools.Controllers
{
    public class vacanciesController : baseController
    {
        public ActionResult Index()
        {
            var viewmodel = new PageViewModel(baseviewmodel);
            var page = repository.GetPage((int) PageType.VACANCY);
            if (page == null)
            {
                viewmodel.content = "No vacancies at this time.";
            }
            else
            {
                viewmodel.content = page.content;
            }
            viewmodel.canEdit = auth.perms.HasFlag(Permission.WEBSITE_EDIT);
            
            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.WEBSITE_EDIT)]
        public ActionResult Edit()
        {
            var viewmodel = new PageViewModel(baseviewmodel);
            var page = repository.GetPage((int)PageType.VACANCY);
            if (page != null)
            {
                viewmodel.content = page.content;
            }

            return View(viewmodel);
        }

        [PermissionFilter(perm = Permission.WEBSITE_EDIT)]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save(string content)
        {
            var page = repository.GetPage((int)PageType.VACANCY);
            if (page == null)
            {
                page = new page();
                repository.AddPage(page);
            }

            page.content = content;

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Page saved successfully".ToJsonOKMessage());
        }
    }
}
