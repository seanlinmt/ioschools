using System.Collections.Generic;
using ioschools.Data.User;
using ioschoolsWebsite.Areas.exams.Models;
using ioschoolsWebsite.Areas.exams.Models.remarks;

namespace ioschoolsWebsite.Models.exam.viewmodels
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