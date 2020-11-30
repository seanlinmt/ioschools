using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ioschools.Library.Extensions.Models;

namespace ioschools.Library.Extensions
{
    public static class SelectExtension
    {
        public static string GroupDropDownList(this HtmlHelper htmlHelper, string name, GroupSelectListItem[] items)
        {
            return GroupDropDownList(htmlHelper, name, items, null);
        }

        public static string GroupDropDownList(this HtmlHelper htmlHelper, string name, GroupSelectListItem[] items,
                                          object htmlAttributes)
        {
            return GroupDropDownList(htmlHelper, name, items, new RouteValueDictionary(htmlAttributes));
        }

        public static string GroupDropDownList(this HtmlHelper htmlHelper, string name, GroupSelectListItem[] items,
                                          IDictionary<string, object> htmlAttributes)
        {
            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("The argument must have a value", "name");
            if (items == null)
                throw new ArgumentNullException("items");
            if (!items.Any())
                throw new ArgumentException("The list must contain at least one value", "items");

            var groups = items.GroupBy(x => x.Group);
            var selectBuilder = new TagBuilder("select");

            var optionbuilder = new StringBuilder();
            foreach (var @group in groups)
            {
                if (string.IsNullOrEmpty(group.Key))
                {
                    foreach (var item in group)
                    {
                        optionbuilder.AppendLine(ListItemToOption(item));
                    }
                }
                else
                {
                    var grouptag = new TagBuilder("optgroup");
                    grouptag.MergeAttribute("label", group.Key);
                    optionbuilder.AppendLine(grouptag.ToString(TagRenderMode.StartTag));
                    foreach (var item in group)
                    {
                        optionbuilder.AppendLine(ListItemToOption(item));
                    }
                    optionbuilder.AppendLine(grouptag.ToString(TagRenderMode.EndTag));
                }
            }
            selectBuilder.InnerHtml = optionbuilder.ToString();
            selectBuilder.MergeAttributes(htmlAttributes);
            selectBuilder.MergeAttribute("name", name);
            selectBuilder.GenerateId(name);

            return selectBuilder.ToString(TagRenderMode.Normal);
        }


        private static string ListItemToOption(SelectListItem item)
        {
            var builder = new TagBuilder("option")
            {
                InnerHtml = HttpUtility.HtmlEncode(item.Text)
            };
            if (item.Value != null)
            {
                builder.Attributes["value"] = item.Value;
            }
            if (item.Selected)
            {
                builder.Attributes["selected"] = "selected";
            }
            return builder.ToString(TagRenderMode.Normal);
        }
    }
}