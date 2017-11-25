using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ioschoolsWebsite.Models.fees;

namespace ioschoolsWebsite.Models.finance
{
    public class FinanceViewModel : BaseViewModel
    {
        public IEnumerable<SelectListItem> schoolList { get; set; }
        public List<LateFeeAlertSiblings> alerts { get; set; }
        public IEnumerable<FeeNotificationTemplate> templates { get; set; }
 
        public FinanceViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            alerts = new List<LateFeeAlertSiblings>();
        }
    }
}