using System.Collections.Generic;
using System.Web.Mvc;
using ioschools.Data.User;
using ioschoolsWebsite.Library.Helpers;
using ioschoolsWebsite.Models.user;

namespace ioschoolsWebsite.Models.enrolment
{
    public class AdmissionViewModel : BaseViewModel
    {
        public string message { get; set; }
        public SelectList designationList { get; set; }
        public IEnumerable<SelectListItem> schools { get; set; } 
        public IEnumerable<SelectListItem> maritalStatusList { get; set; } 

        public AdmissionViewModel(BaseViewModel baseViewModel) : base(baseViewModel)
        {
            designationList = typeof(Designation).ToSelectList(true, "", "");
            maritalStatusList = typeof(MaritalStatus).ToSelectList(false, null, null, MaritalStatus.MARRIED.ToString());
        }
    }
}