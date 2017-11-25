using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NPOI.SS.UserModel;

namespace ioschoolsWebsite.Library.Extensions
{
    public static class NPOIExtension
    {
        public static Cell CreateCellIfNotExist(this Row row, int column)
        {
            return row.GetCell(column) ?? row.CreateCell(column);
        }
    }
}