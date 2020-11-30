using System;
using System.IO;
using System.Web.Mvc;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.ActionFilters;
using ioschools.Library.File;
using ioschools.Library.FileUploader;
using ioschools.Models;

namespace ioschools.Controllers
{
    public class filesController : baseController
    {
        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult Delete(long id)
        {
            try
            {
                var file = repository.GetHomeworkFile(id);
                if (file != null)
                {
                    var path = AppDomain.CurrentDomain.BaseDirectory + file.url;
                    System.IO.File.Delete(path);
                    repository.DeleteHomeworkFile(file);
                }
                
            }
            catch (Exception ex)
            {
                return SendJsonErrorResponse(ex);
            }
            return Json("File deleted successfully".ToJsonOKMessage());

        }

        /// <summary>
        /// so far only used for uploading files for homework
        /// </summary>
        /// <param name="qqfile"></param>
        /// <returns></returns>
        [HttpPost]
        [PermissionFilter(perm = Permission.HOMEWORK_CREATE)]
        public ActionResult Upload(string qqfile)
        {
            var length = long.Parse(Request.Params["CONTENT_LENGTH"]);
            if (sessionid.HasValue)
            {
                var inuse = repository.GetUserDiskUsage(sessionid.Value);
                if (inuse + length > Constants.MAX_DISK_SIZE)
                {
                    return Json("Disk Quota exceeded. Please delete old uploaded files.".ToJsonFail());
                }
            }

            var filename = Path.GetFileNameWithoutExtension(qqfile);
            var ext = Path.GetExtension(qqfile);

            var uploader = new FileHandler(filename.ToSafeUrl() + ext, UploaderType.HOMEWORK, sessionid);
            bool ok = uploader.Save(Request.InputStream);

            if (!ok)
            {
                return Json("An error has occurred. Unable to save file".ToJsonFail());
            }

            // save to database
            var homework_file = new homework_file();
            homework_file.url = uploader.url;
            homework_file.filename = qqfile;
            homework_file.size = uploader.size;
            repository.AddHomeworkFile(homework_file);

            var retVal = new IdName(homework_file.id, uploader.filename);

            return Json(retVal.ToJsonOKData());
        }

    }
}
