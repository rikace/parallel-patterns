using Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelPatterns.Common
{
    public static class Reducer
    {
        public static TResult ReducePartitioner<TValue, TResult>(this IEnumerable<TValue> source,
            Func<TValue, TResult> selector, Func<TResult, TResult, TResult> reducer,
            CancellationToken token = new CancellationToken())
        {
            var partitioner = Partitioner.Create(source, EnumerablePartitionerOptions.None);
            var pos = new ParallelOptions
            {
                TaskScheduler = TaskScheduler.Default,
                CancellationToken = token,
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            
            var results = new AtomImmutable<TResult>(ImmutableArray<TResult>.Empty);
            Parallel.ForEach(partitioner,
                pos,
                () => new List<TResult>(),
                (item, loopState, local) =>
                {
                    local.Add(selector(item));
                    return local;
                },
                final => results.Swap(o => o.AddRange(final))
            );

            return results.Value.AsParallel().Aggregate(reducer);
        }
    }
}