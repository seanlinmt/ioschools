using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Models.user;
using ioschools.DB.repository;

namespace ioschools.Models.fees
{
    public class LateFeeAlertSiblings
    {
        public IEnumerable<LateFeeAlert> children { get; set; }
        public decimal totalPayable { get; set; }

        public LateFeeAlertSiblings(IEnumerable<long> studentids)
        {
            using (var repository = new Repository())
            {
                var overdueStudents =
                repository.GetFees().Where(x => x.status == FeePaymentStatus.UNPAID.ToString() && x.duedate < DateTime.Now && studentids.Contains(x.studentid))
                .GroupBy(x => x.user);


                var alerts = new List<LateFeeAlert>();

                foreach (var entry in overdueStudents)
                {
                    var child = new LateFeeAlert
                                    {
                                        studentname = entry.Key.ToName(false),
                                        studentid = entry.Key.id,
                                        overdueFees = entry.Select(x => x).ToModel().ToArray()
                                    };

                    child.totalUnpaidFees = child.overdueFees.Sum(x => x.amount);

                    alerts.Add(child);
                }

                children = alerts;
            }
            totalPayable = children.SelectMany(x => x.overdueFees).Sum(y => y.amount);
        }

        public LateFeeAlertSiblings()
        {
            
        }
    }
}