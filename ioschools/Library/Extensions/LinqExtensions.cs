using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ioschools.Library.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<TSource> Except<TSource>(this IEnumerable<TSource> first,
        IEnumerable<TSource> second, Func<TSource, TSource, bool> comparer)
        {
            return first.Except(second, new LambdaComparer<TSource>(comparer));
        }
    }
}