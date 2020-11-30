using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschools.Controllers;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.Helpers;
using ioschools.Models.exam.templates;
using ioschools.DB;

namespace ioschools.Areas.exams.Controllers
{
    public class templatesController : baseController
    {
        [NoCache]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE)]
        public ActionResult Add()
        {
            var viewmodel = new ExamTemplateViewModel(baseviewmodel);
            viewmodel.schoolList = repository.GetSchools().Select(
                        x =>
                        new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        });
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE)]
        public ActionResult Delete(int id)
        {
            var template = repository.GetExamTemplate(id);
            if (template == null)
            {
                return Json("Template not found".ToJsonFail());
            }

            if (template.creator.HasValue && template.creator != sessionid.Value)
            {
                return Json("Only creator can delete this template".ToJsonFail());
            }

            try
            {
                db.exam_templates.DeleteOnSubmit(template);

                var subjects = template.exam_template_subjects;
                db.exam_template_subjects.DeleteAllOnSubmit(subjects);

                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Template deleted successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT)]
        public ActionResult DeleteSubject(int id)
        {
            var template = repository.GetExamTemplate(id);
            if (template == null)
            {
                return Json("Template not found".ToJsonFail());
            }

            if (template.creator.HasValue && template.creator != sessionid.Value)
            {
                return Json("Only creator can update this template".ToJsonFail());
            }

            try
            {
                repository.DeleteExamTemplateSubject(id);
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Subject deleted successfully".ToJsonOKMessage());
        }

        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT)]
        public ActionResult Edit(int id)
        {
            var template = repository.GetExamTemplate(id);
            if (template == null)
            {
                return ReturnNotFoundView();
            }

            if (template.creator.HasValue && template.creator.Value != sessionid.Value)
            {
                return ReturnNoPermissionView();
            }

            var viewmodel = new ExamTemplateViewModel(baseviewmodel);
            viewmodel.template = template.ToEditModel();
            viewmodel.schoolList = repository.GetSchools().Select(
                        x =>
                        new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString(),
                            Selected = x.id == viewmodel.template.schoolid
                        });
            return View("Add", viewmodel);
        }


        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT)]
        public ActionResult Save(int? id, string description, bool isprivate, string template_name,
            long?[] subject, string[] subjectname, string[] code, int school, short maxmark, int?[] templatesubjectid)
        {
            var template = new exam_template();
            if (id.HasValue)
            {
                template = repository.GetExamTemplate(id.Value);

                // only allow creator to update
                if (template.creator.HasValue && template.creator.Value != sessionid.Value)
                {
                    return Json("Only the template creator can edit this".ToJsonFail());
                }
            }
            else
            {
                // put things you don't want changed here when editing a template
                template.creator = sessionid.Value;
            }
            template.schoolid = school;
            template.maxMark = maxmark;
            template.name = template_name;
            template.description = description;
            template.isprivate = isprivate;


            if (subjectname != null)
            {
                for (int i = 0; i < subjectname.Length; i++)
                {
                    var sname = subjectname[i];
                    var c = code[i];
                    var s = subject[i];
                    var existingid = templatesubjectid[i];

                    if (string.IsNullOrEmpty(sname))
                    {
                        // ignore blank rows
                        continue;
                    }

                    var entry = new exam_template_subject();
                    if (existingid.HasValue)
                    {
                        entry = template.exam_template_subjects.SingleOrDefault(x => x.id == existingid);
                        if (entry == null)
                        {
                            entry = new exam_template_subject();
                        }
                    }

                    entry.code = c;
                    entry.subjectid = s;
                    entry.name = sname;

                    if (!existingid.HasValue)
                    {
                        template.exam_template_subjects.Add(entry);
                    }
                }
            }

            if (!id.HasValue)
            {
                repository.AddExamTemplate(template);
            }

            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            var viewmodel = template.id.ToJsonOKData();
            viewmodel.message = "Template saved successfully";

            return Json(viewmodel);
        }


        /// <summary>
        /// ordering subjects in exam template
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT)]
        public ActionResult Order(int id, string ids)
        {
            ids = ids.Replace("subject[]=", "");
            var subjects = ids.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var template = repository.GetExamTemplate(id);
            if (template == null)
            {
                return Json("Template not found.".ToJsonFail());
            }

            if (template.creator.HasValue && template.creator.Value != sessionid.Value)
            {
                return Json("Only the creator can update this template".ToJsonFail());
            }

            byte count = 1;
            foreach (var entry in subjects)
            {
                var subject = template.exam_template_subjects.SingleOrDefault(x => x.id.ToString() == entry);
                if (subject != null)
                {
                    subject.position = count++;
                }
            }
            try
            {
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }

            return Json("Order updated successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT | Permission.EXAM_VIEW)]
        public ActionResult Index()
        {
            var viewmodel = new TemplateListViewModel(baseviewmodel);
            viewmodel.templates = repository.GetExamTemplates(sessionid.Value).ToModel();
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT | Permission.EXAM_VIEW)]
        public ActionResult School(int id)
        {
            var data = repository.GetExamTemplates(sessionid.Value).Where(x => x.schoolid == id).ToModel();
            return Json(data.ToJsonOKData());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT | Permission.EXAM_VIEW)]
        public ActionResult Content(int? id)
        {
            var templates = repository.GetExamTemplates(sessionid.Value);
            if (id.HasValue)
            {
                templates = templates.Where(x => x.schoolid == id);
            }
            var viewmodel = templates.ToModel();
            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT)]
        public ActionResult EditRow(int? id, int schoolid)
        {
            var subjects = repository.GetSchoolSubjects(schoolid);

            var viewmodel = new ExamTemplateSubjectViewModel();
            if (id.HasValue)
            {
                var row = db.exam_template_subjects.Single(x => x.id == id.Value);
                viewmodel.id = id.Value.ToString();
                viewmodel.examsubjectname = row.name;
                viewmodel.code = row.code;
                viewmodel.subjects = new[] { new SelectListItem() { Text = "None", Value = "" } }
                    .Union(subjects.Select(x => new SelectListItem()
                    {
                        Text = x.name,
                        Value = x.id.ToString(),
                        Selected = row.subjectid.HasValue && x.id == row.subjectid
                    }));
            }
            else
            {
                viewmodel.subjects = new[] { new SelectListItem() { Text = "None", Value = "" } }.Union(subjects.Select(x => new SelectListItem()
                {
                    Text = x.name,
                    Value = x.id.ToString()
                }));
            }

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT | Permission.EXAM_VIEW)]
        public ActionResult Details(int id)
        {
            var data = repository.GetExamTemplates(sessionid.Value).SingleOrDefault(x => x.id == id);
            if (data == null)
            {
                return Json("Template is invalid".ToJsonFail(), JsonRequestBehavior.AllowGet);
            }

            var viewmodel = data.ToEditModel();

            var view = this.RenderViewToString("Details", viewmodel);

            return Json(view.ToJsonOKData());

        }

        [HttpPost]
        [PermissionFilter(perm = Permission.EXAM_ADMIN | Permission.EXAM_CREATE | Permission.EXAM_EDIT | Permission.EXAM_VIEW)]
        public ActionResult SubjectRows(int id)
        {
            var template = repository.GetExamTemplate(id);
            if (template == null)
            {
                return new EmptyResult();
            }

            var viewmodel = template.exam_template_subjects.OrderBy(x => x.position).ToModel();
            return View(viewmodel);
        }
    }
}
