using System;
using System.Collections.Generic;

namespace RabCab.Engine.Enumerators
{
    public static class IEnumerables
    {
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            int count;
            try
            {
                count = source.Count();
            }
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }
    }
}