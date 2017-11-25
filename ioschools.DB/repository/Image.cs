using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public long AddBlogImage(blog_image img)
        {
            db.blog_images.InsertOnSubmit(img);
            db.SubmitChanges();
            return img.id;
        }

        public long AddUserImage(user_image image)
        {
            db.user_images.InsertOnSubmit(image);
            db.SubmitChanges();
            return image.id;
        }


        
        public void DeleteUserImage(long id)
        {
            var image = db.user_images.Where(x => x.id == id).SingleOrDefault();
            if (image != null)
            {
                db.user_images.DeleteOnSubmit(image);
                Save();
            }
        }
    }
}
