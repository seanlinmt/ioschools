using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Library.Imaging;
using ioschools.Models.photo;

namespace ioschools.Models.user
{
    public class UserBase
    {
        public string id { get; set; }
        public string name { get; set; }
        public string thumbnailString { get; set; }
        public string classname { get;set; }

        public UserBase()
        {
            id = "";
        }
    }

    public static class UserBaseHelper
    {
        public static UserBase ToBaseModel(this ioschools.DB.user row)
        {
            return new UserBase()
                       {
                           name = row.name,
                           id = row.id.ToString(),
                           thumbnailString = row.photo.HasValue
                                                 ? Img.by_size(row.user_image.url, Imgsize.USER_THUMB).ToHtmlImage()
                                                 : Img.PHOTO_NO_THUMBNAIL.ToHtmlImage(),
                       };
        }

        public static IEnumerable<UserBase> ToBaseModel(this IEnumerable<ioschools.DB.user> rows)
        {
            foreach (var row in rows)
            {
                yield return row.ToBaseModel();
            }
        }

        public static IEnumerable<UserBase> ToBaseStudentModel(this IEnumerable<ioschools.DB.user> rows, int year)
        {
            foreach (var row in rows)
            {
                yield return new UserBase()
                                 {
                                     name = row.name,
                                     id = row.id.ToString(),
                                     thumbnailString = row.photo.HasValue
                                                           ? Img.by_size(row.user_image.url, Imgsize.USER_THUMB).
                                                                 ToHtmlImage()
                                                           : Img.PHOTO_NO_THUMBNAIL.ToHtmlImage(),
                                     classname = row.classes_students_allocateds.First(x => x.year == year).school_class.name
                                 };
            }
            
        }
    }
}