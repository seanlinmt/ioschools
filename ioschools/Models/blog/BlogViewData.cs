using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using ioschools.Data.CMS;
using ioschools.Models.photo;

namespace ioschools.Models.blog
{
    public class BlogViewData : BaseViewModel
    {
        public BlogViewData(BaseViewModel baseviewdata): base(baseviewdata)
        {
            photos = Enumerable.Empty<Photo>();
            attachments = Enumerable.Empty<IdNameUrl>();
            blog = new Blog();
        }

        public Blog blog { get; set; }
        public IEnumerable<SelectListItem> schools { get; set; } 

        // thumbnails
        public IEnumerable<Photo> photos { get; set; }
        public LayoutMethod method { get; set; }

        // attachments
        public IEnumerable<IdNameUrl> attachments { get; set; }

    }
}