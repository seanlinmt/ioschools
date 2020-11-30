using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using clearpixels.Logging;

namespace ioschools.Library
{
    public static class Utility
    {
        public static string ToNumbersOnly(this string str)
        {
            return Regex.Replace(str, @"[^0-9]", "");
        }

        public static string ToSafeFilename(this string str)
        {
            return Regex.Replace(str, @"[\\/:\*\?<>|]", "-");
        }

        public static string ToSafeUrl(this string str)
        {
            // to replace a bunch of -- with just one -
            Regex removeDuplicateDashRegex = new Regex("(-){2,}");
            return removeDuplicateDashRegex.Replace(Regex.Replace(str, @"[^\w]", "-"), "-");
        }

        public static bool IsPowerOfTwo(this int x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }

        public static string GeneratePasswordHash(string email, string password)
        {
            return BCrypt.HashPassword(email + password, BCrypt.GenerateSalt());
        }

        public static DateTime GetDBDate()
        {
            var date = DateTime.Now;
            return new DateTime(date.Year, date.Month, date.Day);
        }

        public static bool HasUpper(this string str)
        {
            var regex = new Regex("[A-Z]+");
            var match = regex.Match(str);
            return match.Success;
        }

        public static string NumberToText(int n)
        {
            if (n < 0)
                return "Minus " + NumberToText(-n);
            else if (n == 0)
                return "";
            else if (n <= 19)
                return new string[] {"One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", 
         "Nine", "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", 
         "Seventeen", "Eighteen", "Nineteen"}[n - 1] + " ";
            else if (n <= 99)
                return new string[] {"Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", 
         "Eighty", "Ninety"}[n / 10 - 2] + " " + NumberToText(n % 10);
            else if (n <= 199)
                return "One Hundred " + NumberToText(n % 100);
            else if (n <= 999)
                return NumberToText(n / 100) + "Hundred " + NumberToText(n % 100);
            else if (n <= 1999)
                return "One Thousand " + NumberToText(n % 1000);
            else if (n <= 999999)
                return NumberToText(n / 1000) + "Thousand " + NumberToText(n % 1000);
            else if (n <= 1999999)
                return "One Million " + NumberToText(n % 1000000);
            else if (n <= 999999999)
                return NumberToText(n / 1000000) + "Million " + NumberToText(n % 1000000);
            else if (n <= 1999999999)
                return "One Billion " + NumberToText(n % 1000000000);
            else
                return NumberToText(n / 1000000000) + "Billion " + NumberToText(n % 1000000000);
        }

        public static bool SaveFile(Stream fileStream, string destName)
        {
            try
            {
                var destination = AppDomain.CurrentDomain.BaseDirectory + destName;
                // delete if exists
                if (System.IO.File.Exists(destination))
                {
                    Syslog.Write(ErrorLevel.ERROR, "Existing file overwritten: " + destination);
                    System.IO.File.Delete(destination);
                }

                using (var file = System.IO.File.Create(destination))
                {
                    byte[] buffer = new byte[8 * 1024];
                    int len;
                    while ((len = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        file.Write(buffer, 0, len);
                    }
                }
            }
            catch (Exception ex)
            {
                Syslog.Write(ErrorLevel.CRITICAL, "Unable to save file: " + destName + " " + ex.Message);
                return false;
            }
            return true;
        }

        public static string StripHtmlTags(this string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            return Regex.Replace(html, @"<(.|\n)*?>", string.Empty);
        }

        // the following does not handle nested tags well
        public static string ToFirstParagraphText(this string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            var regex = new Regex(@"<p[^>]*>([\w\W]*?)</p>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var match = regex.Match(html);
            if (!match.Success)
            {
                return "";
            }
            return match.Groups[1].Value;
        }

        public static string ToHtmlBreak(this string original)
        {
            var htmlstring = "";
            if (string.IsNullOrEmpty(original))
            {
                return "";
            }
            htmlstring = original.Replace("\n", "<br />");

            return htmlstring;
        }

        public static string ToHtmlParagraph(this string original)
        {
            var htmlstring = "";
            if (string.IsNullOrEmpty(original))
            {
                return "";
            }
            htmlstring = "<p>" +
                         original.Replace(Environment.NewLine + Environment.NewLine, "</p><p>")
                                    .Replace(Environment.NewLine, "<br/>")
                                    .Replace("</p><p>", "</p>" + Environment.NewLine + "<p>")
                         + "</p>";


            return htmlstring;
        }
    }
}