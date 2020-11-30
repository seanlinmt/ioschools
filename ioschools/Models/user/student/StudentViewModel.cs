using System.Collections.Generic;
using System.Linq;
using ioschools.Models.discipline;
using ioschools.Models.exam;
using ioschools.Models.homework.viewmodels;
using ioschools.Models.schedule;

namespace ioschools.Models.user.student
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