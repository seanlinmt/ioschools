using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ioschools.Library.Helpers
{
    public static class SelectListHelper
    {
        public static SelectList ToSelectList(this IEnumerable<SelectListItem> values)
        {
            return values.ToSelectList(null);
        }

        public static SelectList ToSelectList(this IEnumerable<SelectListItem> values, object selectedValue)
        {
            return values.ToSelectList(selectedValue, "None", "");
        }

        public static SelectList ToSelectList(this IEnumerable<SelectListItem> values, 
            object selectedValue, string emptyText, string emptyValue)
        {
            var result = values.ToList();
            result.Insert(0, new SelectListItem { Text = emptyText, Value = emptyValue });
            if (selectedValue == null)
            {
                return new SelectList(result, "Value", "Text");
            }
            return new SelectList(result, "Value", "Text", selectedValue);
        }

        public static SelectList ToSelectList(this Type type, bool useDescrAsVal, string emptyText, string emptyValue, string selectedValue = "")
        {
            return type.ToSelectList(useDescrAsVal, emptyText, emptyValue, true, selectedValue);
        }

        public static SelectList ToSelectList(this Type type, bool useDescrAsVal, string emptyText, string emptyValue, bool order, string selectedValue)
        {
            var enumvalues = Enum.GetValues(type);
            List<SelectListItem> values = new List<SelectListItem>();
            foreach (Enum value in enumvalues)
            {
                values.Add(new SelectListItem()
                               {
                                   Text = value.ToDescriptionString(),
                                   Value = useDescrAsVal?value.ToDescriptionString(): value.ToString()
                               });
            }

            List<SelectListItem> includeDash;
            if (order)
            {
                includeDash = values.OrderBy(x => x.Text).ToList();
            }
            else
            {
                includeDash = values.ToList();
            }

            if (emptyText != null)
            {
                includeDash.Insert(0, new SelectListItem {Text = emptyText, Value = emptyValue});    
            }

            return new SelectList(includeDash, "Value", "Text", selectedValue);
        }
    }
}