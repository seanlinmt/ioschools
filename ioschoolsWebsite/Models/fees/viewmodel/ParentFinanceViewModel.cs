using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschoolsWebsite.Models.finance;

namespace ioschoolsWebsite.Models.fees.viewmodel
{
    public class ParentFinanceViewModel : BaseViewModel
    {
        public LateFeeAlertSiblings alert { get; set; }
        public StatementViewModel statement { get; set; }

        public ParentFinanceViewModel(BaseViewModel baseviewmodel) : base(baseviewmodel)
        {
            
        }
    }
}