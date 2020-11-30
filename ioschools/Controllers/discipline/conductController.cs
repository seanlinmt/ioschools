using System;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.discipline;
using ioschools.DB;

namespace ioschools.Controllers.discipline
{
    // used mostly for admin side
    public class conductController : baseController
    {
        [HttpGet]
        [PermissionFilter(perm = Permission.CONDUCT_ADMIN)]
        public ActionResult AdminContent(int? id)
        {
            if (id.HasValue)
            {
                return View(new[] {db.conducts.Single(x => x.id == id.Value).ToModel()});
            }
            var viewmodel = db.conducts.ToModel();

            return View(viewmodel);
        }


        [HttpPost]
        [PermissionFilter(perm = Permission.CONDUCT_ADMIN)]
        public ActionResult Delete(long id)
        {
            try
            {
                var l = db.conducts.Single(x => x.id == id);
                db.conducts.DeleteOnSubmit(l);
                repository.Save();
            }
            catch (Exception ex)
            {
                return Json("Unable to delete conduct type. Entry is in use.".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.CONDUCT_ADMIN)]
        public ActionResult Edit(long? id)
        {
            var viewmodel = new Conduct();
            if (id.HasValue)
            {
                viewmodel = db.conducts.Single(x => x.id == id.Value).ToModel();
            }

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.CONDUCT_ADMIN)]
        public ActionResult Save(int? id, string name, bool isdemerit, short? min, short? max)
        {
            // if min/max specified then both needs to be specified
            if ((min.HasValue && !max.HasValue) ||
                (!min.HasValue && max.HasValue))
            {
                return Json("Min and max values must be specified together".ToJsonFail());
            }

            if (min.HasValue && max.HasValue)
            {
                if (min.Value >= max.Value)
                {
                    return Json("Min value has to be less than Max value".ToJsonFail());
                }
            }

            var single = new conduct();
            if (id.HasValue)
            {
                single = db.conducts.Single(x => x.id == id.Value);
            }
            single.name = name;
            single.isdemerit = isdemerit;
            single.min = min;
            single.max = max;

            if (!id.HasValue)
            {
                db.conducts.InsertOnSubmit(single);
            }

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var viewmodel = "Entry saved successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("AdminContent", new[] { single.ToModel() });

            return Json(viewmodel);
        }

    }
}
