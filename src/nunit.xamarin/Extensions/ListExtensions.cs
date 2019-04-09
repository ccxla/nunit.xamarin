using System;
using System.Collections.Generic;

namespace nunit.xamarin.Extensions
{
    public static class ListExtensions
    {
        public static IEnumerable<IList<T>> Partition<T>(
            this IEnumerable<T> source,
            int size)
        {
            if (size < 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Must be greater or equal to 2.");
            }

            T[] partition;
            int count;

            using (var e = source.GetEnumerator())
            {
                if (e.MoveNext())
                {
                    partition = new T[size];
                    partition[0] = e.Current;
                    count = 1;
                }
                else
                {
                    yield break;
                }

                while (e.MoveNext())
                {
                    partition[count] = e.Current;
                    count++;

                    if (count == size)
                    {
                        yield return partition;
                        count = 0;
                        partition = new T[size];
                    }
                }
            }

            if (count > 0)
            {
                Array.Resize(ref partition, count);
                yield return partition;
            }
        }

        public static void Shuffle<T>(this IList<T> list, Random rng)
        {
            var n = list.Count;
            while (n > 1)
            {
                n--;
                var k = rng.Next(n + 1);
                var value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
