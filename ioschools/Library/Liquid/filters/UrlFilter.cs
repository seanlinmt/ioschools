using System;

namespace ioschools.Library.Liquid.filters
{
    public static class UrlFilter
    {
        public static string img_tag(string input, string alt = "")
        {
            return String.Format("<img src='{0}' alt='{1}' />", input, alt ?? "");
        }

        public static string link_to(string input, string url)
        {
            return string.Format("<a title='{0}' href='{1}'>{0}</a>", input, url);
        }

        public static string script_tag(string input, string theme_version)
        {
            return String.Format("<script type='text/javascript' src='{0}?v={1}'></script>", input, theme_version);
        }

        public static string stylesheet_tag(string input, string theme_version)
        {
            return String.Format("<link media='all' type='text/css' rel='stylesheet' href='{0}?v={1}'>", input, theme_version);
        }
    }
}