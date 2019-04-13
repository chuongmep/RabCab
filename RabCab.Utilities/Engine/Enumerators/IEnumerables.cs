using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabCab.Engine.Enumerators
{
    public static class IEnumerables
    {
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            int count;
            try
            {
                count = source.Count<TSource>();
            }
            catch (Exception)
            {
                count = 0;
            }

            return count;
        }
    }
}
