using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.DB;
using ioschools.Library.Imaging;

namespace ioschools.Models.photo
{
    public class Photo
    {
        public long id { get; set; }
        public string url { get; set; }
        public string url_fullsize { get; set; }
    }

    public static class PhotoHelper
    {
        public static IEnumerable<Photo> ToModel(this IEnumerable<blog_image> images, Imgsize size, Imgsize fullsize = Imgsize.MAX)
        {
            foreach (var image in images)
            {
                yield return new Photo()
                {
                    id = image.id,
                    url = Img.by_size(image.url, size),
                    url_fullsize = Img.by_size(image.url, fullsize)
                };
            }
        }

        public static IEnumerable<Photo> ToModel(this IQueryable<user_image> images, Imgsize size, Imgsize fullsize = Imgsize.MAX)
        {
            foreach (var image in images)
            {
                yield return image.ToModel(size, fullsize);
            }
        }

        public static Photo ToModel(this user_image img, Imgsize size, Imgsize fullsize = Imgsize.MAX)
        {
            return new Photo()
            {
                id = img.id,
                url = Img.by_size(img.url, size),
                url_fullsize = Img.by_size(img.url, fullsize)
            };
        }
    }
}