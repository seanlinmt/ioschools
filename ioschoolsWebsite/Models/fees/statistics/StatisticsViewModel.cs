using System.Collections.Generic;
using System.Web;

namespace ioschoolsWebsite.Models.fees.statistics
{
    public class StatisticsViewModel
    {
        public string statname { get; set; }
        public List<FeeRow> feetypes { get; set; }

        public StatisticsViewModel()
        {
            feetypes = new List<FeeRow>();
        }
    }
}