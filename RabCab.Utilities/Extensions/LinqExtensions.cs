using System;
using System.Collections.Generic;
using System.Linq;

namespace RabCab.Extensions
{
    public static class LinqExtensions
    {
        /// <summary>
        ///     Checks whether all items in the enumerable are same (Uses <see cref="object.Equals(object)" /> to check for
        ///     equality)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable">The enumerable.</param>
        /// <returns>
        ///     Returns true if there is 0 or 1 item in the enumerable or if all items in the enumerable are same (equal to
        ///     each other) otherwise false.
        /// </returns>
        public static bool AreAllSame<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));

            using (var enumerator = enumerable.GetEnumerator())
            {
                var toCompare = default(T);
                if (enumerator.MoveNext()) toCompare = enumerator.Current;

                while (enumerator.MoveNext())
                    if (toCompare != null && !toCompare.Equals(enumerator.Current))
                        return false;
            }

            return true;
        }

        public static bool AreListsSame<T>(this List<List<T>> iList)
        {
            if (iList.Count <= 0) return false;
            if (iList.Count == 1) return true;

            var baseList = iList.First();

            foreach (var lst in iList)
                if (!lst.SequenceEqual(baseList))
                    return false;

            return true;
        }

        public static List<List<T>> ChunkBy<T>(this List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}