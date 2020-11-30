using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.blog
{
    public class NewsPanelViewModel
    {
        public IEnumerable<BlogSummary> news { get; set; }
        public int page { get; set; }
        public bool hasOlder { get; set; }
        public bool hasNewer { get; set; }

        public NewsPanelViewModel()
        {
            news = Enumerable.Empty<BlogSummary>();
        }
    }
}