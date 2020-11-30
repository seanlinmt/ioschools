using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ioschools.Models.homework.viewmodels
{
    public class HomeworkViewModel : BaseViewModel
    {
        public bool editable { get; set; }
        public float DiskspaceLeft { get; set; }
        public float DiskspaceUsed { get; set; }
        public IEnumerable<SelectListItem> subjects { get; set; }
        public IEnumerable<Homework> homeworks { get; set; }

        public HomeworkViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            homeworks = Enumerable.Empty<Homework>();
        }
    }
}