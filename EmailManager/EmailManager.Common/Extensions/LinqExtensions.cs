using System.Collections.Generic;
using System.Linq;

namespace EmailManager.Common.Extensions
{
    public static class LinqExtensions
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }

        public static List<List<T>> Split<T>(this List<T> list, int parts)
        {
            int i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.ToList();
            return splits.ToList();
        }
    }
}
