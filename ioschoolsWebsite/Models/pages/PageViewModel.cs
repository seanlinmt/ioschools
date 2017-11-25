using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschoolsWebsite.Models.pages
{
    public class PageViewModel : BaseViewModel
    {
        public string content { get; set; }
        public bool canEdit { get; set; }
        public PageViewModel(BaseViewModel baseViewModel) : base(baseViewModel)
        {
            
        }

    }
}