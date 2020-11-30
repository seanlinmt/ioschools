using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace ioschools.Library.Helpers
{
    public static class EnumHelper
    {
        public static List<string> GetDescriptionStrings(this Type obj)
        {
            var retVal = new List<string>();
            if (!obj.IsEnum)
            {
                throw new InvalidEnumArgumentException();
            }
            foreach (FieldInfo info in obj.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                var attribs = (DescriptionAttribute[])info.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var descr = attribs[0].Description;
                Debug.Assert(!string.IsNullOrEmpty(descr));
                retVal.Add(descr);
            }
            return retVal;
        }

        /// <summary>
        /// Returns string specified by DescriptionAttribute, otherwise do the usual ToString()
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToDescriptionString(this Enum obj)
        {
            var attribs = (DescriptionAttribute[])obj.GetType().GetField(obj.ToString()).GetCustomAttributes(typeof (DescriptionAttribute), false);
            return attribs.Length > 0 ? attribs[0].Description : obj.ToString();
        }

        public static int ToInt(this Enum obj)
        {
            return Convert.ToInt32(obj);
        }

        public static T ToEnum<T>(this int obj)
        {
            return obj.ToString().ToEnum<T>();
        }

        public static T ToEnum<T>(this byte obj)
        {
            return obj.ToString().ToEnum<T>();
        }

        public static T ToEnum<T>(this string obj)
        {
            if (string.IsNullOrEmpty(obj))
            {
                return default(T);
            }
            try
            {
                return (T)Enum.Parse(typeof(T), obj, true);
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }
}