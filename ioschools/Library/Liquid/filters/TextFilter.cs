using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ioschools.Library.Liquid.filters
{
    public static class TextFilter
    {
        public static string highlight(string input, string textToHighlight)
        {
            return Regex.Replace(input, @"(" + textToHighlight + ")", "<strong class=highlight>$1</strong>", RegexOptions.IgnoreCase);
        }

        public static string paragraphs(string input)
        {
            return input.ToHtmlParagraph();
        }

        public static string number_to_text(string input)
        {
            var abs = Convert.ToInt32(Math.Abs(decimal.Parse(input, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint)));

            return Utility.NumberToText(abs);
        }

        public static string newline_to_br(string input)
        {
            return input.ToHtmlBreak();
        }

        public static string pluralize(long input, string single, string plural)
        {
            if (input == 1)
            {
                return single;
            }

            return plural;
        }
    }

}