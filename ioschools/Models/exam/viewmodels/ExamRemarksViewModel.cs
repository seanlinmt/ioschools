using System.Collections.Generic;
using ioschools.Data.User;
using ioschools.Areas.exams.Models;
using ioschools.Areas.exams.Models.remarks;

namespace ioschools.Models.exam.viewmodels
{
    public class ExamRemarksViewModel
    {
        public List<StudentRemark> remarks { get; set; }
        public bool canEdit { get; set; }

        public ExamRemarksViewModel(Permission viewer_perms)
        {
            remarks = new List<StudentRemark>();
            canEdit = (viewer_perms & (Permission.TRANSCRIPTS_CREATE | Permission.TRANSCRIPTS_EDIT)) != 0;
        }
    }
}