using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace ioschools.Library.Extensions
{
    public static class SelectYearExtension
    {
        public static string SelectYear(this HtmlHelper htmlHelper, string name, int count)
        {
            var sb = new StringBuilder();
            sb.AppendFormat("<select id='{0}' name='{0}'>", name);
            var year = DateTime.Now.Year - count + 1;
            for (int i = 0; i < count; i++)
            {
                sb.AppendFormat("<option value='{0}'>{0}</option>", year + i);
            }

            sb.AppendFormat("</select>");

            return sb.ToString();
        }
    }
}