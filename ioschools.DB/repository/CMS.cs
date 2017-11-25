using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ioschools.Data;

namespace ioschools.DB.repository
{
    public partial class Repository
    {
        public void AddBlog(blog blog)
        {
            db.blogs.InsertOnSubmit(blog);
        }

        public long AddBlogFile(blog_file file)
        {
            db.blog_files.InsertOnSubmit(file);
            Save();
            return file.id;
        }

        public void DeleteBlog(long id)
        {
            var blog = db.blogs.Where(x => x.id == id).SingleOrDefault();
            if (blog != null)
            {
                db.blogs.DeleteOnSubmit(blog);
            }
        }

        public void DeleteBlogFile(long id)
        {
            var exist = db.blog_files.Where(x => x.id == id).Single();
            db.blog_files.DeleteOnSubmit(exist);
        }

        public void DeleteBlogPhoto(long id)
        {
            var exist = db.blog_images.Where(x => x.id == id).Single();
            db.blog_images.DeleteOnSubmit(exist);
        }

        public blog GetBlog(long id)
        {
            return db.blogs.Where(x => x.id == id).SingleOrDefault();
        }

        public blog_image GetBlogPhoto(long id)
        {
            return db.blog_images.Where(x => x.id == id).SingleOrDefault();
        }

        public blog_file GetBlogFile(long id)
        {
            return db.blog_files.Where(x => x.id == id).SingleOrDefault();
        }

        public IEnumerable<blog> GetBlogs()
        {
            return db.blogs;
        }

        public IEnumerable<blog> GetBlogs(BlogType? type, int? year)
        {
            var blogs = db.blogs.AsQueryable();
            if (type.HasValue)
            {
                switch (type.Value)
                {
                    case BlogType.INTERNAL:
                        blogs = blogs.Where(x => !x.ispublic);
                        break;
                    case BlogType.PUBLIC:
                        blogs = blogs.Where(x => x.ispublic);
                        break;
                }
            }

            if (year.HasValue)
            {
                blogs = blogs.Where(x => x.created.Year == year.Value);
            }

            return blogs;
        }

        public void UpdateBlogFiles(long id, string[] files)
        {
            var result =
                db.blog_files.Where(x => files.Contains(x.id.ToString()));
            foreach (var row in result)
            {
                row.blogid = id;
            }
            Save();
        }

        public void UpdateBlogImages(long blogid, IEnumerable<string> photoids)
        {
            var result =
                db.blog_images.Where(x => photoids.Contains(x.id.ToString()));
            foreach (var row in result)
            {
                row.blogid = blogid;
            }
            Save();
        }

        public page GetPage(int typeid)
        {
            return db.pages.Where(x => x.type == typeid).SingleOrDefault();
        }

        public void AddPage(page page)
        {
            db.pages.InsertOnSubmit(page);
        }
    }
}
