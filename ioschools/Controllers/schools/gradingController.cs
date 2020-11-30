using System;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.school.json;
using clearpixels.Logging;
using ioschools.DB;

namespace ioschools.Controllers.schools
{
    [PermissionFilter(perm = Permission.EXAM_ADMIN)]
    public class gradingController : baseController
    {
        [HttpGet]
        public ActionResult Add()
        {
            var viewmodel = new GradingJSON();

            return View("Edit", viewmodel);
        }

        [HttpGet]
        public ActionResult AddRule()
        {
            var viewmodel = new GradingRuleJSON();

            return View("EditRuleRows", new[]{viewmodel});
        }


        [HttpPost]
        public ActionResult Delete(int id)
        {
            var single = db.grades_methods.Single(x => x.id == id);
            var rules = single.grades_rules;
            try
            {
                db.grades_rules.DeleteAllOnSubmit(rules);
                db.grades_methods.DeleteOnSubmit(single);
                repository.Save();
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return Json("Failed to delete entry".ToJsonFail());
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var viewmodel = db.grades_methods.Single(x => x.id == id);

            return View(viewmodel.ToModel());
        }

        [HttpGet]
        public ActionResult EditRuleRows(int? id)
        {
            if (id.HasValue)
            {
                var viewmodel = db.grades_methods
                    .Single(x => x.id == id)
                    .grades_rules
                    .OrderByDescending(x => x.mark)
                    .ToModel();

                return View(viewmodel);
            }

            return View(Enumerable.Empty<GradingRuleJSON>());
        }


        [HttpGet]
        public ActionResult Rows(int? id)
        {
            if (id.HasValue)
            {
                return View(db.grades_methods.Where(x => x.id == id.Value).ToModel());
            }
            return View(db.grades_methods.OrderBy(x => x.name).ToModel());
        }

        [HttpGet]
        public ActionResult Rules(int id)
        {
            var rules = db.grades_methods.Single(x => x.id == id)
                .grades_rules
                .OrderByDescending(x => x.mark)
                .ToModel();
            return View(rules);
        }


        [HttpPost]
        [JsonFilter(Param = "method", RootType = typeof(GradingJSON))]
        public ActionResult Save(GradingJSON method)
        {
            var single = new grades_method();
            if (method.id.HasValue)
            {
                single = db.grades_methods.Single(x => x.id == method.id.Value);
                
                // delete existing
                var postedIDs = method.rules.Where(y => y.id.HasValue).Select(y => y.id.Value).ToArray();
                var todelete = single.grades_rules.Where(x => !postedIDs.Contains(x.id));
                db.grades_rules.DeleteAllOnSubmit(todelete);

                // update existing
                foreach (var entry in method.rules.Where(x => x.id.HasValue))
                {
                    var rule = single.grades_rules.Single(x => x.id == entry.id.Value);
                    rule.grade = entry.grade;
                    rule.gradepoint = entry.gradepoint;
                    if (!entry.mark.HasValue)
                    {
                        return Json("A mark must be specified for each rule added".ToJsonFail());
                    }
                    rule.mark = entry.mark.Value;
                }
            }
            else
            {
                db.grades_methods.InsertOnSubmit(single);
            }
            single.name = method.name;

            // insert new
            foreach (var entry in method.rules.Where(x => !x.id.HasValue))
            {
                var rule = new grades_rule();
                rule.grade = entry.grade;
                rule.gradepoint = entry.gradepoint;
                if (!entry.mark.HasValue)
                {
                    return Json("A mark must be specified for each rule added".ToJsonFail());
                }
                rule.mark = entry.mark.Value;
                single.grades_rules.Add(rule);
            }

            repository.Save();

            var viewmodel = "Entry saved successfully".ToJsonOKMessage();
            viewmodel.data = this.RenderViewToString("Rows", new[]{single.ToModel()});

            return Json(viewmodel);
        }
    }
}
