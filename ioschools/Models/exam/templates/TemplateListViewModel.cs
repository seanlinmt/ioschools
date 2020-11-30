using System.Collections.Generic;

namespace ioschools.Models.exam.templates
{
    public class TemplateListViewModel : BaseViewModel
    {
        public IEnumerable<ExamTemplate> templates { get; set; }

        public TemplateListViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }
    }
}