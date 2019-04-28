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
            Func<T, Task> action,
            int degreeOfParallelism)
        {
            // TODO
            // Implement logic that runs the "action" for
            // each item "collection" with degree of parallelism "degreeOfParallelism"
            
            Task[] tasks = null;
            await Task.WhenAll(tasks);
        }

        public static async Task<R[]> ExecuteInParallel<T, R>(this IEnumerable<T> collection,
            Func<T, Task<R>> processor,
            int degreeOfParallelism)
        {
            Task<R>[] tasks = null;

            // TODO
            // Implement logic that runs the "projection" for
            // each item "collection" with degree of parallelism "degreeOfParallelism"
            
            var results = await Task.WhenAll(tasks.ToList());
            return results;
        }

    }
}