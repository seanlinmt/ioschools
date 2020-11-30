using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace clearpixels.Models.jqgrid
{
    public class JqgridTable
    {
        public int page { get; set; }
        public int total { get; set; }
        public int records { get; set; }

        public IList<JqgridRow> rows { get; set; }
        public Dictionary<string, object> userdata { get; set; }

        public JqgridTable()
        {
            rows = new List<JqgridRow>();
            userdata = new Dictionary<string, object>();
        }
    }
}