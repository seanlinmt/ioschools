using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using Microsoft.CSharp;

namespace ioschools.Library.Helpers
{
    public static class StringHelper
    {
        public static string ToLiteral(this string input)
        {
            var writer = new StringWriter();
            var provider = new CSharpCodeProvider();
            provider.GenerateCodeFromExpression(new CodePrimitiveExpression(input), writer, null);
            return writer.GetStringBuilder().ToString();
        }
    }
}