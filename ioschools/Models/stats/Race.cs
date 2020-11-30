using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Models.stats
{
    public class Race
    {
        public string name { get; set; }
        public int male { get; set; }
        public int female { get; set; }
        public int unknown { get; set; }

        public Race()
        {
            name = "";
        }
    }
}