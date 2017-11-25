using System.Collections.Generic;
using System.Linq;
using ioschoolsWebsite.Models.discipline;
using ioschoolsWebsite.Models.exam;
using ioschoolsWebsite.Models.homework.viewmodels;
using ioschoolsWebsite.Models.schedule;

namespace ioschoolsWebsite.Models.user.student
{
    public class StudentViewModel : BaseViewModel
    {
        public HomeworkStudentViewModel homework { get; set; }
        public ExamStudentViewModel exam { get; set; }
        public IEnumerable<Schedule> schedules { get; set; }
        public DisciplineViewModel discipline { get; set; }

        public StudentViewModel(BaseViewModel viewmodel) : base(viewmodel)
        {
            schedules = Enumerable.Empty<Schedule>();
        }
    }
}