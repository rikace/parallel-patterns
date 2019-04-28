using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace ParallelPatterns.Common
{
    public static class Helpers
    {
        private static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
                @this.Add(element);
        }
        
        public static bool AddRange<T>(this HashSet<T> @this, IEnumerable<T> items)
        {
            bool allAdded = true;
            foreach (T item in items)
                allAdded &= @this.Add(item);
            return allAdded;
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> col)
            => col.SelectMany(l => l);

        public static HashSet<T> AsSet<T>(this IEnumerable<T> col)
            => new HashSet<T>(col);

        public static HashSet<string> AsSet(this IEnumerable<string> col)
            => new HashSet<string>(col, StringComparer.OrdinalIgnoreCase);
    }
}