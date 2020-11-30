using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Data.User;
using ioschools.Library.Helpers;
using ioschools.Models.fees;
using ioschools.DB;

namespace ioschools.Models.finance
{
    public class StatementViewModel
    {
        public string year { get; set; }
        public DateTime discountDate { get; set; }
        public bool showDiscountRow { get; set; }
        public List<StatementChildren> childs { get; set; }
        public string totalPayable { get; set; }
        public string totalPayableDiscounted { get; set; }

        public StatementViewModel(ioschools.DB.user parent, int year, Permission perms)
        {
            childs = new List<StatementChildren>();
            this.year = year.ToString();

            var childFees = parent.students_guardians1
                .SelectMany(x => x.user.fees)
                .Where(y => y.duedate.Year == year);

            if (childFees.Count() != 0)
            {
                discountDate = childFees.Min(x => x.duedate);

                showDiscountRow = DateTime.Now <= discountDate || ((Permission.FEES_ADMIN | Permission.FEES_UPDATE_STATUS) & perms) != 0;

                foreach (var entry in childFees.GroupBy(y => y.user))
                {
                    var statement = new StatementChildren();
                    statement.childname = entry.Key.ToName();
                    statement.fees = entry.Select(x => x).ToModel(perms);
                    childs.Add(statement);
                }

                var payable = (float)childs.SelectMany(x => x.fees)
                    .Where(y => y.status != FeePaymentStatus.PAID.ToDescriptionString())
                    .Sum(y => y.amount);

                if (payable > 0)
                {
                    totalPayable = payable.ToString("n2");
                    totalPayableDiscounted = (payable * 0.97).ToString("n2");
                }
            }

            
        }
    }
}