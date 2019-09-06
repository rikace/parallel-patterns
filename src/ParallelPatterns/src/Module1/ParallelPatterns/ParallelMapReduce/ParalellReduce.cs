using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Helpers;

namespace DataParallelism.Reduce
{
    using Helpers;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Threading;

    // TODO : 2.2
    // implement two parallel Reducer functions
    // requirements
    // 1 - reduce all the items in a collection starting from the first one
    // 3 - reduce all the items in a collection starting from a given initial value
    // Suggestion, look into the LINQ Aggregate
    // You could implement two different functions with different signatures
    // Tip : there are different ways to implement a parallel reducer, even using a Parallel For loop
    public static class ParallelReducer
    {
        // public static TValue Reduce<TValue>

        // parallel Reduce function implementation using Aggregate
        // Example of signature, but something is missing
        // public static TValue Reduce<TValue>(this IEnumerable<TValue> source) =>
        public static TValue Reduce<TValue>(this ParallelQuery<TValue> source, Func<TValue, TValue, TValue> func) => default(TValue);

        public static TValue Reduce<TValue>(this IEnumerable<TValue> source, TValue seed,
           Func<TValue, TValue, TValue> reduce) => default(TValue);

        public static TResult[] Reduce<TSource, TKey, TMapped, TResult>(
            this IEnumerable<IGrouping<TKey, TMapped>> source, Func<IGrouping<TKey, TMapped>, TResult> reduce) => null;


        public static TResult ReducePartitioner<TValue, TResult>(this IEnumerable<TValue> source,
          Func<TValue, TResult> selector, Func<TResult, TResult, TResult> reducer, CancellationToken token = new CancellationToken())
      => default(TResult);
    }
}
