using System.Collections.Generic;

namespace ioschools.Models.fees.statistics
{
    public class FeeRow
    {
        public string feename { get; set; }
        public List<StatRow> entries { get; set; }

        public FeeRow()
        {
            entries = new List<StatRow>();
        }
    }
}