using System;
using System.Collections.Generic;
using System.Linq;

namespace VinhLB.Utilities
{
    public static class CollectionExtensions
    {
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            Random random = ThreadSafeRandom.Instance;

            return source.OrderBy(_ => random.Next());
        }

        public static void Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }
    }
}