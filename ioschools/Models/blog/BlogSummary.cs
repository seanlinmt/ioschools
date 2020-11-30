using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ioschools.Data;
using ioschools.Library;
using ioschools.Models.calendar;

namespace ioschools.Models.blog
{
    public class BlogSummary
    {
        public string id { get; set; }
        public string date { get; set; }
        public string title { get; set; }
        public string content { get; set; }
    }

    public static class BlogSummaryHelper
    {
        public static IEnumerable<BlogSummary> ToModel(this IEnumerable<CalendarEntry> values)
        {
            foreach (var entry in values)
            {
                yield return new BlogSummary()
                {
                    date = new DateTime(entry.year, entry.month, entry.day).ToString(Constants.DATEFORMAT_DATEPICKER),
                    title = string.Join(", ", entry.entry.ToArray())
                };
            }
        }

        private delegate string ParagraphTruncater(string s);

        public static IEnumerable<BlogSummary> ToModel(this IEnumerable<ioschools.DB.blog> values)
        {
            ParagraphTruncater paragraph = delegate(string x)
                        {
                            // we want to truncate to end of sentence
                            var p = x.ToFirstParagraphText().StripHtmlTags().Trim();
                            if (string.IsNullOrEmpty(p))
                            {
                                p = x.StripHtmlTags().Trim();
                            }
                            var lengthToTruncate = Math.Min(100, p.Length);
                            if (p.Length > 100)
                            {
                                var fullstopIndex = p.IndexOfAny(new[]{'.',':',';'}, 100);
                                if (fullstopIndex != -1)
                                {
                                    lengthToTruncate = fullstopIndex + 1;
                                }
                            }
                            return p.Substring(0, lengthToTruncate);
                        };

            foreach (var entry in values)
            {
                yield return new BlogSummary()
                                 {
                                     date = entry.created.ToString(Constants.DATEFORMAT_DATEPICKER),
                                     title = entry.title,
                                     id = entry.id.ToString(),
                                     content = paragraph(entry.body)
                };
            }
        }
    }
}