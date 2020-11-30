using System;
using System.IO;
using System.Threading;
using System.Web.Mvc;
using ioschools.DB;
using ioschools.Library;
using ioschools.Library.File;
using ioschools.Library.FileUploader;
using ioschools.Library.Imaging;
using ioschools.Models.photo;

namespace ioschools.Controllers
{
    public class photoController : baseController
    {
        [HttpPost]
        public void Delete(long id)
        {
            if (sessionid.HasValue)
            {
                repository.DeleteUserImage(id);
            }
        }

        [HttpPost]
        public ActionResult Upload(long? id, string qqfile)
        {
            var uploader = new FileHandler(qqfile, UploaderType.PHOTO, null);

            var thisImage = new user_image();
            thisImage.url = uploader.url;

            bool ok = uploader.Save(Request.InputStream);
            if (!ok)
            {
                return Json("An error has occurred. Unable to save image".ToJsonFail());
            }

            // save to database
            var imageid = repository.AddUserImage(thisImage);

            var retVal = new Photo();
            string thumbnailUrl = Img.by_size(thisImage.url, Imgsize.USER_PROFILE);
            if (id.HasValue)
            {
                var usr = repository.GetUser(id.Value);
                if (usr.photo.HasValue)
                {
                    // we need to delete the old photo from the system
                    var url = usr.user_image.url;
                    new Thread(() => Img.delete(url)).Start();
                }
                usr.user_image = thisImage;
                repository.Save();
            }
            retVal.id = imageid;
            retVal.url = thumbnailUrl;

            return Json(retVal.ToJsonOKData());
        }

    }
}
