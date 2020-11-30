using System;
using System.IO;
using ioschools.Data;
using ioschools.Library.FileUploader;
using ioschools.Library.Imaging;
using clearpixels.Logging;

namespace ioschools.Library.File
{
    public class FileHandler
    {
        public string filename { get; private set; }
        public string url { get; private set; }
        public long size { get; private set; }

        public FileHandler(string filename, UploaderType type, long? ownerid)
        {
            this.filename = filename;
            string folder = "";
            switch (type)
            {
                case UploaderType.CALENDAR:
                    folder = "Content/media/";
                    break;
                case UploaderType.BLOG:
                    folder = Constants.UPLOAD_PATH_BLOG + DateTime.UtcNow.Ticks + "/";
                    break;
                case UploaderType.PHOTO:
                    var extIndex = filename.LastIndexOf('.');
                    var ext = filename.Substring(extIndex);
                    filename = ImgHelper.BuildFilename(ext);
                    folder = Constants.UPLOAD_PATH + DateTime.UtcNow.DayOfYear + "/";
                    break;
                case UploaderType.HOMEWORK:
                    folder = Constants.UPLOAD_PATH_HOMEWORK + ownerid + "/" + DateTime.UtcNow.Ticks + "/";
                    break;
                case UploaderType.REGISTRATION:
                    folder = Constants.UPLOAD_PATH_REGISTRATION + ownerid + "/" + DateTime.UtcNow.Ticks + "/";
                    break;
            }
            if (!Directory.Exists(AppDomain.CurrentDomain.BaseDirectory + folder))
            {
                Directory.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory + folder);
            }
            url = string.Concat("/", folder, filename);
        }

        public bool Save(Stream filestream)
        {
            try
            {
                Utility.SaveFile(filestream, url);
                // get filesize
                var fileinfo = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + url);
                size = fileinfo.Length;
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return false;
            }
            finally
            {
                filestream.Flush();
                filestream.Close();
            }
            return true;
        }

        public static void Delete(string path)
        {
            var mainloc = AppDomain.CurrentDomain.BaseDirectory + path;
            if (System.IO.File.Exists(mainloc))
            {
                System.IO.File.Delete(mainloc);
            }
        }
    }
}