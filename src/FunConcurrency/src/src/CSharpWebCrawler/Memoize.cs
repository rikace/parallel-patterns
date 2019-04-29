using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCrawler
{
    public static class Memoization
    {
        public static Func<T, R> Memoize<T, R>(Func<T, R> func) where T : IComparable
        {
            Dictionary<T, R> cache = new Dictionary<T, R>();
            return arg =>
            {
                if (cache.ContainsKey(arg))
                    return cache[arg];
                return (cache[arg] = func(arg));
            };
        }


        // TODO 
        // (1) Implement Thread-safe memoization function
        // (2) Optionally, implement memoization with Lazy behavior
//        public static Func<T, R> MemoizeThreadSafe<T, R>(Func<T, R> func) where T : IComparable
//        {
//            return null;
//        }
       public static Func<T, Task<R>> MemoizeLazyThreadSafe<T, R>(Func<T, Task<R>> func) where T : IComparable
        {
            ConcurrentDictionary<T, Lazy<Task<R>>> cache = new ConcurrentDictionary<T, Lazy<Task<R>>>();
            return arg => cache.GetOrAdd(arg, a => new Lazy<Task<R>>(() => func(a))).Value;
        }

    }
}
