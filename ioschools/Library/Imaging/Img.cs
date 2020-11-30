using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using clearpixels.Logging;

namespace ioschools.Library.Imaging
{
    public class Img
    {

        public const string PHOTO_NO_THUMBNAIL = "/Content/img/users/profile_nophoto.png";

        /// <summary>
        /// returns path to thumbnail, if thumbnail does not exist it creates it then returns the new path
        /// </summary>
        /// <param name="filePath">the path to the thumbnail, usually stored in db</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string by_size(string filePath, Imgsize type) 
        {
            var dim = getImageDimensionsFromSize(type);
            int width = dim.Width;
            int height = dim.Height;
            // appends dimension to path
            string suffix = width + "x" + height;
            string ofile;
            string thumb = ofile = filePath;
            string ext = thumb.Substring(thumb.LastIndexOf(".") + 1);
            string part1 = thumb.Substring(0, thumb.LastIndexOf("."));
            if (part1.IndexOf(".") == -1)
            {
                thumb = part1 + "." + suffix + "." + ext;
            }
            else
            {
                string part2 = part1.Substring(0, part1.LastIndexOf("."));
                thumb = part2 + "." + suffix + "." + ext;
            }
            bool fileExist = false;

            if (!fileExist)
            {
                fileExist = System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + thumb);
            }
            if (!fileExist)
            {
                thumb = thumbnail(ofile, suffix, width, height, type);
            }

            return thumb;
        }

        private static Image createResizedImage(Image originalImage, Size poSize, Imgsize type)
        {
            //Detach image from its source
            Image oImageOriginal = (Image)originalImage.Clone();

            //Resize new image
            var oResizedImage = new Bitmap(poSize.Width, poSize.Height);
            Graphics oGraphic = Graphics.FromImage(oResizedImage);

            oGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;
            oGraphic.CompositingQuality = CompositingQuality.HighSpeed;
            oGraphic.SmoothingMode = SmoothingMode.HighSpeed;
            oGraphic.InterpolationMode = InterpolationMode.Low;
            Rectangle oRectangle = new Rectangle(0, 0, poSize.Width, poSize.Height);

            oGraphic.DrawImage(oImageOriginal, oRectangle);

            // cleanup
            oGraphic.Dispose();
            
            oImageOriginal.Dispose();
            return oResizedImage;
        }

        public static void delete(string path)
        {
            // go through each image size
            var sizes = Enum.GetValues(typeof (Imgsize));
            foreach (Imgsize size in sizes)
            {
                var loc = AppDomain.CurrentDomain.BaseDirectory + by_size(path, size);
                bool exists = System.IO.File.Exists(loc);
                if (exists)
                {
                    System.IO.File.Delete(loc);
                }
            }

            // finally delete the main image
            var mainloc = AppDomain.CurrentDomain.BaseDirectory + path;
            if (System.IO.File.Exists(mainloc))
            {
                System.IO.File.Delete(mainloc);
            }
        }

        private static Size getImageDimensionsFromSize(Imgsize size)
        {
            switch (size)
            {
                case Imgsize.THUMB:
                    return new Size(200, 300);
                case Imgsize.BLOG:
                    return new Size(800, 500);
                case Imgsize.GALLERY:
                    return new Size(75, 75);
                case Imgsize.MAX:
                    return new Size(900, 675);
                case Imgsize.USER_THUMB:
                    return new Size(50, 50);
                case Imgsize.USER_PROFILE:
                    return new Size(270, 270);
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// generates a thumbnail given the path of the original images
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="suffix"></param>
        /// <param name="desiredWidth"></param>
        /// <param name="desiredHeight"></param>
        /// <returns></returns>
        private static string thumbnail(string filePath, string suffix, float desiredWidth, float desiredHeight, Imgsize type) 
        {
            string thumb = filePath;
            string file = filePath;
            string ext = thumb.Substring(thumb.LastIndexOf(".") + 1);
            thumb = thumb.Substring(0, thumb.IndexOf(".")) + "." + suffix + "." + ext;
            bool exists = System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + file);
            if (!exists) 
            {
                //Syslog.Write(ErrorLevel.ERROR, string.Concat("Cannot find file: ", AppDomain.CurrentDomain.BaseDirectory + file));
                return "";
            }
            // These are the ratio calculations
            int width;
            int height;
            Image img = null;
            try
            {
                img = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + file);
                width = img.Width;
                height = img.Height;
            }
            catch (OutOfMemoryException)
            {
                Syslog.Write(ErrorLevel.ERROR, "Invalid image: " + filePath);
                return "";
            }
            finally
            {
                if (img != null)
                {
                    img.Dispose();
                }
            }
            
            float factor = 0;
            if (width > 0 && height > 0) 
            {
                float wfactor = desiredWidth / width;
                float hfactor = desiredHeight / height;
                factor = wfactor < hfactor ? wfactor : hfactor;
            }
            if (factor != 0 && factor < 1) 
            {
                int twidth = Convert.ToInt32(Math.Floor(factor * width));
                int theight = Convert.ToInt32(Math.Floor(factor * height));
                convert(file, thumb, twidth, theight, type);
            } 
            else
            {
                if (System.IO.File.Exists(AppDomain.CurrentDomain.BaseDirectory + thumb))
                {
                    System.IO.File.Delete(AppDomain.CurrentDomain.BaseDirectory + thumb);
                }
                System.IO.File.Copy(AppDomain.CurrentDomain.BaseDirectory + file, AppDomain.CurrentDomain.BaseDirectory + thumb);
            }
            return thumb;
        }

        private static void convert(string source, string destination, int desiredWidth, int desiredHeight, Imgsize type) 
        {
            createImage(source, destination, desiredWidth, desiredHeight, type);
        }

        private static bool createImage(string srcName, string destName, int desiredWidth, int desiredHeight, Imgsize type) 
        {
            var source = AppDomain.CurrentDomain.BaseDirectory + srcName;
            var destination = AppDomain.CurrentDomain.BaseDirectory + destName;
            // Capture the original size of the uploaded image
            Image src = null;
            try
            {
                src = Image.FromFile(source);
            }
            catch (Exception ex)
            {
                if (src != null)
                {
                    src.Dispose();
                }
                Syslog.Write(ex);
                throw;
            }
            
            //Resize new image
            //Image tmp = src.GetThumbnailImage(desiredWidth, desiredHeight, null, IntPtr.Zero);
            Size imgSize = new Size(desiredWidth,desiredHeight);
            Image tmp = createResizedImage(src, imgSize, type);

            try
            {
                System.IO.File.Delete(destination);
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
            }

            try
            {
                tmp.Save(destination, destination.ToImageFormat());
            }
            catch (Exception ex)
            {
                Syslog.Write(ex);
                return false;
            }
            finally
            {
                src.Dispose();
                tmp.Dispose();
            }
           
            return true;
        }

        public static bool SaveImage(Stream imageStream, string destName)
        {
            try
            {
                var destination = AppDomain.CurrentDomain.BaseDirectory + destName;
                // delete if exists
                if (System.IO.File.Exists(destination))
                {
                    Syslog.Write(ErrorLevel.ERROR, "Existing image overwritten: " + destination);
                    System.IO.File.Delete(destination);
                }
                var image = Bitmap.FromStream(imageStream);
                image.Save(destination, destName.ToImageFormat());
            }
            catch (Exception ex)
            {
                Syslog.Write(ErrorLevel.CRITICAL, "Unable to save image: " + destName + " " + ex.Message);
                return false;
            }
            return true;
        }
    }

    public static class ImgHelper
    {
        public static string BuildFilename(string extension)
        {
            return string.Concat(DateTime.UtcNow.Ticks, extension);
        }

        public static bool IsImage(this string filename)
        {
            var extIndex = filename.LastIndexOf('.');
            var ext = filename.Substring(extIndex);
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                case ".JPG":
                case "JPEG":
                case ".png":
                case ".PNG":
                case ".gif":
                case ".GIF":
                    return true;
                default:
                    return false;
            }
        }

        public static ImageFormat ToImageFormat(this string filename)
        {
            var extIndex = filename.LastIndexOf('.');
            var ext = filename.Substring(extIndex);
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                case ".JPG":
                case "JPEG":
                    return ImageFormat.Jpeg;
                case ".png":
                case ".PNG":
                    return ImageFormat.Png;
                case ".gif":
                case ".GIF":
                    return ImageFormat.Gif;
                default:
                    // 14/11: commented out because gbase will flood log
                    //Syslog.Write(ErrorLevel.INFORMATION, string.Concat("Unrecognised image extension: ", filename));
                    return ImageFormat.Jpeg;
            }
        }

        private static string ToDataUriType(this string filename)
        {
            var extIndex = filename.LastIndexOf('.');
            var ext = filename.Substring(extIndex);
            switch (ext)
            {
                case ".jpg":
                case ".jpeg":
                case ".JPG":
                case "JPEG":
                    return "data:image/jpeg;base64,";
                case ".png":
                case ".PNG":
                    return "data:image/png;base64,";
                case ".gif":
                case ".GIF":
                    return "data:image/gif;base64,";
                default:
                    throw new Exception(string.Concat("Unrecognised image extension for datauri: ", filename));
            }
        }
    }
}