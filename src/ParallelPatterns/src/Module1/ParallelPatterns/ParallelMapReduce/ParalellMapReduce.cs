using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataParallelism.MapReduce
{
    using Helpers;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;

    // TODO : Implement a Map-Reduce Function (as extension method - reusable)
    public static class ParallelMapReduce
    {
        // (1)
        // start with Map, follow this method signature
        // The IGrouping is achieved with the keySelector function, this is arbitrary and you can implement the Map function without it
        public static IEnumerable<IGrouping<TKey, TMapped>> Map<TSource, TKey, TMapped>(this IList<TSource> source,
            Func<TSource, IEnumerable<TMapped>> map, Func<TMapped, TKey> keySelector)
        {
            // replace null with the implementation
            return null;
        }

        // (2)
        // Implement the reduce function, this is a suggested signature but you can simplified it and/or expanded it
        public static TResult[] Reduce<TSource, TKey, TMapped, TResult>(
            this IEnumerable<IGrouping<TKey, TMapped>> source, Func<IGrouping<TKey, TMapped>, TResult> reduce)
        {
            // replace null with the implementation
            return null;
        }

        // (3)
        // Compose the pre-implemented Map and Reduce function
        // After the implementation of the Map-Reduce
        // - How can you control/manage the degree of parallelism ?
        // - Improve the performance with a Partitioner
        // Suggestion, for performance improvement look into the "WithExecutionMode"
        public static TResult[] MapReduce<TSource, TMapped, TKey, TResult>(this IList<TSource> source,
            Func<TSource, IEnumerable<TMapped>> map, Func<TMapped, TKey> keySelector,
            Func<IGrouping<TKey, TMapped>, TResult> reduce)
        {
            return null;
        }

        public static TResult[] MapReduce<TSource, TMapped, TKey, TResult>(
           this IList<TSource> source,
           Func<TSource, IEnumerable<TMapped>> map,
           Func<TMapped, TKey> keySelector,
           Func<IGrouping<TKey, TMapped>, TResult> reduce,
           int M, int R)
        {
            return null;
        }

        public static ParallelQuery<TResult> MapReduce<TSource, TMapped, TKey, TResult>(
            this ParallelQuery<TSource> source,
            Func<TSource, IEnumerable<TMapped>> map,
            Func<TMapped, TKey> keySelector,
            Func<IGrouping<TKey, TMapped>, IEnumerable<TResult>> reduce)
        {
            return null;
        }
    }
}