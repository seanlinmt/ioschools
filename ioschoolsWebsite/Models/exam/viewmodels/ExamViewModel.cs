using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ioschoolsWebsite.Models.exam.statistics;
using ioschoolsWebsite.Models.exam.templates;
using ioschoolsWebsite.Models.subject;

namespace ioschoolsWebsite.Models.exam.viewmodels
{
    public class ExamViewModel : BaseViewModel
    {
        public Exam exam { get; set; }
        public string description { get; set; }

        // stats
        public IEnumerable<string> grades { get; set; }
        public Dictionary<string, List<ExamStat>> stats { get; set; } // key = subject, values = all stats for subject
        public IEnumerable<string> class_names { get; set; }
        public IEnumerable<Subject> subjects { get; set; }
        public bool iscreator { get; set; }

        // exam subjects
        public IEnumerable<ExamTemplateSubjectViewModel> examsubjects { get; set; } 

        public ExamViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            stats = new Dictionary<string, List<ExamStat>>();
            grades = Enumerable.Empty<string>();
        }
    }
}