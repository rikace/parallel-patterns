using System;
using System.Collections.Generic;
using System.Linq;

namespace ParallelPatterns.Common
{
    public static class Helpers
    {
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
        
        
        public static Func<T1, Func<T2, R>> Curry<T1, T2, R>(this Func<T1, T2, R> func)
            => t1 => t2 => func(t1, t2);

        public static Func<T1, Func<T2, T3, R>> Curry<T1, T2, T3, R>
            (this Func<T1, T2, T3, R> @this) => t1 => (t2, t3) => @this(t1, t2, t3);

        public static Func<T1, T3> Compose<T1, T2, T3>(Func<T1, T2> f1, Func<T2, T3> f2) => a => f2(f1(a));
    }
}