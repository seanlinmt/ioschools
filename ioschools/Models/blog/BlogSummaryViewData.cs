using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.blog
{
    public class BlogSummaryViewData : BaseViewModel
    {
        public NewsPanelViewModel newspanel { get; set; }
        public IEnumerable<BlogSummary> events { get; set; }

        public BlogSummaryViewData(BaseViewModel baseviewdata) :base(baseviewdata)
        {
            events = Enumerable.Empty<BlogSummary>();
            newspanel = new NewsPanelViewModel();
        }
    }
}