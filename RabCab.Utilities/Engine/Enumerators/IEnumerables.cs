using System;
using System.Collections.Generic;

namespace RabCab.Engine.Enumerators
{
    public static class Enumerables
    {
        public static int CountT<TSource>(this IEnumerable<TSource> source)
        {
            int count;
            try
            {
                count = source.CountT();
            }
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }
    }
}