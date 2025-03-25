using System;
using System.Collections.Generic;

namespace AncientLunar.Extensions
{
    internal static class EnumeratorMax
    {
        public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
            where TKey : IComparable<TKey>
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));

            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                    throw new InvalidOperationException("Sequence contains no elements");

                TSource maxElement = enumerator.Current;
                TKey maxKey = selector(maxElement);

                while (enumerator.MoveNext())
                {
                    TSource candidate = enumerator.Current;
                    TKey candidateKey = selector(candidate);

                    if (candidateKey.CompareTo(maxKey) > 0)
                    {
                        maxElement = candidate;
                        maxKey = candidateKey;
                    }
                }

                return maxElement;
            }
        }
    }
}
