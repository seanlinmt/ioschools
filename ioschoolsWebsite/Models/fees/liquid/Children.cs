using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotLiquid;

namespace ioschoolsWebsite.Models.fees.liquid
{
    public class Children : Drop
    {
        public string name { get; set; }
        public string @class { get; set; }
    }
}