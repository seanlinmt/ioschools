using System.Collections.Generic;
using System.Web.Mvc;

namespace ioschoolsWebsite.Models.enrolment
{
    public class EnrolmentViewModel : BaseViewModel
    {
        public Enrolment enrolment { get; set; }

        public EnrolmentViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }
    }
}