using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockAnalyzer
{
    public static class ExecuteInWithDegreeOfParallelism
    {

        public static async Task ExecuteInParallel<T>(this IEnumerable<T> collection,
            Func<T, Task> processor,
            int degreeOfParallelism)
        {
            Task[] tasks = null;
            await Task.WhenAll(tasks);
        }

        public static async Task<R[]> ExecuteInParallel<T, R>(this IEnumerable<T> collection,
            Func<T, Task<R>> processor,
            int degreeOfParallelism)
        {
            Task<R>[] tasks = null;

            var results = await Task.WhenAll(tasks.ToList());
            return results;
        }

        public static Task ForEachAsyncConcurrent<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            var partitions = Partitioner.Create(source).GetPartitions(dop);
            var tasks = partitions.Select(async partition =>
            {
                using (partition)
                    while (partition.MoveNext())
                        await body(partition.Current);
            });

            return Task.WhenAll(tasks);
        }
    }
}