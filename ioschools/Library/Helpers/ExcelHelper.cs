using System;
using NPOI.SS.UserModel;

namespace ioschools.Library.Helpers
{
    public static class ExcelHelper
    {
        public static bool AsBoolean(this ICell cell)
        {
            bool value = false;
            if (cell != null)
            {
                try
                {
                    value = cell.BooleanCellValue;
                }
                catch
                {

                }
            }

            return value;
        }

        public static int? AsInt(this ICell cell)
        {
            int? value = null;
            if (cell != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(cell.StringCellValue))
                    {
                        value = int.Parse(cell.StringCellValue);
                    }
                }
                catch
                {
                    // if error then cell is double
                    value = Convert.ToInt32(cell.NumericCellValue);
                }
            }

            return value;
        }

        public static string AsString(this ICell cell)
        {
            string value = "";
            if (cell != null)
            {
                try
                {
                    value = cell.StringCellValue;
                }
                catch
                {
                    // if error then cell is numeric
                    value = cell.NumericCellValue.ToString();
                }
            }

            return value;
        }
    }
}