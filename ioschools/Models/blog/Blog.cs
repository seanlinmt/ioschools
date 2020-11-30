using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using ioschools.Data;
using ioschools.DB;
using ioschools.Library;
using ioschools.Models.user;
using clearpixels.Models.jqgrid;

namespace ioschools.Models.blog
{
    public class Blog : BlogSummary
    {
        public string pagetitle { get; set; }
        public string creator { get; set; }
        public string body { get; set; }
        public bool ispublic { get; set; }
    }

    public static class BlogHelper
    {
        public static Blog ToModel(this ioschools.DB.blog row)
        {
            return new Blog()
                       {
                           id = row.id.ToString(),
                           pagetitle = row.title,
                           title = row.title,
                           creator = row.user.ToName(),
                           body = row.body,
                           ispublic = row.ispublic,
                           date = row.created.ToString(Constants.DATEFORMAT_DATEPICKER)
                       };
        }

        public static JqgridTable ToBlogsJqGrid(this IEnumerable<ioschools.DB.blog> rows)
        {
            var grid = new JqgridTable();
            foreach (var row in rows)
            {
                var entry = new JqgridRow();
                entry.id = row.id.ToString();
                entry.cell = new object[]
                                 {
                                     row.id,
                                     string.Format("<a class='bold' target='_blank' href='/news/{3}/{4}'>{0}</a><div class='font_grey'>{1} - {2}</div>", 
                                     row.title,
                                     row.created.ToString(Constants.DATETIME_SHORT_DATE),
                                     row.user.ToName(false),
                                     row.id,
                                     row.title.ToSafeUrl()),
                                     row.ToStatus(),
                                     "<span class='jqedit'>edit</span><span class='jqdelete'>delete</span>"
                                 };
                grid.rows.Add(entry);
            }
            return grid;
        }

        private static string ToStatus(this ioschools.DB.blog row)
        {
            var sb = new StringBuilder();

            if (row.ispublic)
            {
                sb.AppendFormat("<span class='tag_green'>public</span>");
            }
            else
            {
                sb.AppendFormat("<span class='tag_red'>unpublished</span>");
            }

            if(row.emailed)
            {
                sb.AppendFormat("<span class='tag_blue'>emailed</span>");
            }

            return sb.ToString();
        }

        public static IEnumerable<IdNameUrl> ToModel(this IEnumerable<blog_file> rows)
        {
            foreach (var row in rows)
            {
                yield return new IdNameUrl()
                                 {
                                     id = row.id.ToString(),
                                     name = row.name,
                                     url = row.url
                                 };
            }
        }
    }
}