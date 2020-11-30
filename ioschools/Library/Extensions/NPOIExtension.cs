using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;

namespace ioschools.Library.Extensions
{
    public static class NPOIExtension
    {
        public static ICell CreateCellIfNotExist(this IRow row, int column)
        {
            return row.GetCell(column) ?? row.CreateCell(column);
        }
    }
}