using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.CMS;
using ioschools.Data.User;
using clearpixels.Logging;
using ioschools.DB;
using ioschools.DB.repository;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.email;
using ioschools.Library.File;
using ioschools.Library.FileUploader;
using ioschools.Library.Helpers;
using ioschools.Library.Imaging;
using ioschools.Models.blog;
using ioschools.Models.photo;
using ioschools.Models.user;

namespace ioschools.Controllers.blog
{
    public class blogController : baseController
    {
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Add()
        {
            var viewmodel = new BlogViewData(baseviewmodel);
            viewmodel.blog.pagetitle = "New Circular / News";
            viewmodel.schools = new[] { new SelectListItem() { Text = "All schools", Value = "" } }.Union(
                    db.schools
                        .OrderBy(x => x.id)
                        .Select(x => new SelectListItem()
                        {
                            Text = x.name,
                            Value = x.id.ToString()
                        }));

            return View(viewmodel);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Delete(long id)
        {
            try
            {
                // get blog
                var blog = repository.GetBlog(id);
                if (blog == null)
                {
                    return Json("Entry not found".ToJsonFail());
                }

                if (sessionid.Value != blog.creator && !auth.perms.HasFlag(Permission.NEWS_ADMIN))
                {
                    return Json("Only creator or admin can delete this entry".ToJsonFail());
                }

                // delete files
                foreach (var entry in blog.blog_files)
                {
                    var file = entry;
                    new Thread(x =>FileHandler.Delete(file.url)).Start();
                    repository.DeleteBlogFile(file.id);
                }

                // delete images
                foreach (var entry in blog.blog_images)
                {
                    var image = entry;
                    new Thread(x => Img.delete(image.url)).Start();
                    repository.DeleteBlogPhoto(image.id);
                }

                // delete blog
                repository.DeleteBlog(id);

                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("Entry deleted successfully".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult DeletePhoto(long id)
        {
            try
            {
                var photo = repository.GetBlogPhoto(id);
                Img.delete(photo.url);
                repository.DeleteBlogPhoto(id);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("ok".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult DeleteAttachment(long id)
        {
            try
            {
                var file = repository.GetBlogFile(id);
                FileHandler.Delete(file.url);
                repository.DeleteBlogFile(id);
                repository.Save();
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("ok".ToJsonOKMessage());
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult List(BlogType? type, int? year, int rows, int page)
        {
            var results = repository.GetBlogs(type, year);

            // return in the format required for jqgrid
            results = results.OrderByDescending(x => x.id);
            var blogs = results.Skip(rows * (page - 1)).Take(rows).ToBlogsJqGrid();
            var records = results.Count();
            var total = (records / rows);
            if (records % rows != 0)
            {
                total++;
            }

            blogs.page = page;
            blogs.records = records;
            blogs.total = total;

            return Json(blogs);
        }

        [HttpPost]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Upload(long? id, string qqfile)
        {
            // determine file type
            var isimage = qqfile.IsImage();

            var filename = Path.GetFileNameWithoutExtension(qqfile);
            var ext = Path.GetExtension(qqfile);

            if (isimage)
            {
                var uploader = new FileHandler(filename.ToSafeUrl() + ext, UploaderType.PHOTO, sessionid);

                var thisImage = new blog_image();
                thisImage.url = uploader.url;

                bool ok = uploader.Save(Request.InputStream);
                if (!ok)
                {
                    return Json("An error has occurred. Unable to save blog image".ToJsonFail());
                }

                // save to database
                var imageid = repository.AddBlogImage(thisImage);

                string thumbnailUrl = Img.by_size(thisImage.url, Imgsize.USER_PROFILE);
                if (id.HasValue)
                {
                    thisImage.blogid = id.Value;
                    repository.Save();
                }

                return Json(new
                                {
                                    id = imageid,
                                    url = thumbnailUrl
                                }.ToJsonOKData());
            }
            else
            {
                var uploader = new FileHandler(filename.ToSafeUrl() + ext, UploaderType.BLOG, sessionid);

                var file = new blog_file();
                file.url = uploader.url;
                file.name = qqfile;

                bool ok = uploader.Save(Request.InputStream);
                if (!ok)
                {
                    return Json("An error has occurred. Unable to save blog file".ToJsonFail());
                }

                // save to database
                var fileid = repository.AddBlogFile(file);

                if (id.HasValue)
                {
                    file.blogid = id.Value;
                    repository.Save();    
                }

                return Json(new{ id = fileid, url = file.url, name = file.name}.ToJsonOKData());
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Save(long? id, string title, string photoids, string attachmentids,
                    string content, Schools? school, bool test = false, bool blog_public = false, bool blog_email = false, bool blog_gallery = false)
        {
            if (test)
            {
                // just send test email
                var viewdata = new ViewDataDictionary
                                       {
                                           {"title", title},
                                           {"content", content},
                                           {"sender", repository.GetUser(sessionid.Value).ToName()}
                                       };
                var usr = repository.GetUser(sessionid.Value);
                EmailHelper.SendEmailNow(
                                EmailViewType.CIRCULAR,
                                viewdata,
                                title,
                                usr.email,
                                usr.ToName());
                return Json("A test email has been sent to your account".ToJsonOKMessage());
            }

            var blog = new ioschools.DB.blog
                           {
                               created = DateTime.Now, 
                               creator = sessionid.Value
                           };

            if (id.HasValue)
            {
                blog = repository.GetBlog(id.Value);
            }

            blog.title = title;
            blog.body = content;
            blog.ispublic = blog_public;
            

            if (blog_gallery)
            {
                blog.layoutMethod = (byte) LayoutMethod.GALLERY;
            }
            else
            {
                blog.layoutMethod = (byte) LayoutMethod.SIDE;
            }

            if (!id.HasValue)
            {
                repository.AddBlog(blog);
            }
            
            try
            {
                repository.Save();

                // update photos
                if (!string.IsNullOrEmpty(photoids))
                {
                    var photos = photoids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    repository.UpdateBlogImages(blog.id, photos); // submit
                }
                

                // update attachments
                if (!string.IsNullOrEmpty(attachmentids))
                {
                    var files = attachmentids.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    repository.UpdateBlogFiles(blog.id, files); // submit
                }

                // don't send if it's draft
                // check if it has already been sent
                if (blog_email && blog_public && !blog.emailed)
                {
                    // do we have permission
                    if (!auth.perms.HasFlag(Permission.NEWS_BROADCAST))
                    {
                        return Json("You do not have permission to send circulars".ToJsonFail());
                    }

                    // update sent
                    blog.emailed = blog_email;
                    repository.Save();

                    var viewdata = new ViewDataDictionary
                                       {
                                           {"title", title},
                                           {"content", content},
                                           {"sender", repository.GetUser(sessionid.Value).ToName()}
                                       };
                    new Thread(() =>
                                   {
                                       using (var repo = new Repository())
                                       {
                                           IQueryable<user> usrs = null;
                                           if (school.HasValue)
                                           {
                                               usrs = repo.GetUsers(null, null, school.Value.ToInt(), null, null, null, null, null, DateTime.Now.Year, null);
                                           }

                                           if (usrs != null)
                                           {
                                               foreach (var usr in usrs)
                                               {
                                                   // don't send to parents whoose children has not been accepted
                                                   if (usr.usergroup == (int)UserGroup.GUARDIAN)
                                                   {
                                                       if (!usr.students_guardians1.Select(x => x.user.classes_students_allocateds).Any())
                                                       {
                                                           continue;
                                                       }
                                                   }

                                                   if (!string.IsNullOrEmpty(usr.email))
                                                   {
                                                       EmailHelper.SendEmail(
                                                           EmailViewType.CIRCULAR,
                                                           viewdata,
                                                           title,
                                                           usr.email,
                                                           usr.ToName());
                                                   }
                                               }
                                           }
                                       }
                                   }).Start();
                }
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return Json("Unable to create entry.".ToJsonFail());
            }

            return Json("Entry updated successfully".ToJsonOKMessage());
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Edit(long id)
        {
            var blog = repository.GetBlog(id);
            if (blog == null)
            {
                return ReturnNotFoundView();
            }

            if (sessionid.Value != blog.creator && !auth.perms.HasFlag(Permission.NEWS_ADMIN))
            {
                return Json("Only creator or admin can edit this entry".ToJsonFail());
            }

            var viewmodel = new BlogViewData(baseviewmodel)
                                {
                                    blog = blog.ToModel(),
                                    photos = blog.blog_images.ToModel(Imgsize.USER_PROFILE),
                                    attachments = blog.blog_files.ToModel(),
                                    method =
                                        blog.layoutMethod.HasValue
                                            ? (LayoutMethod) blog.layoutMethod.Value
                                            : LayoutMethod.NONE,
                                    schools = new[] {new SelectListItem() {Text = "All schools", Value = ""}}.Union(
                                        db.schools
                                            .OrderBy(x => x.id)
                                            .Select(x => new SelectListItem()
                                                             {
                                                                 Text = x.name,
                                                                 Value = x.id.ToString()
                                                             }))
                                };
            
            return View("Add", viewmodel);
        }

        [HttpGet]
        [PermissionFilter(perm = Permission.NEWS_ADMIN | Permission.NEWS_CREATE)]
        public ActionResult Index()
        {
            return View(baseviewmodel);
        }

    }
}
