using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncOperations
{
    public static class ExecuteInWithDegreeOfParallelism
    {
        public static async Task ExecuteInParallel<T>(this IEnumerable<T> collection,
            Func<T, Task> processor,
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
            // TODO
            // Implement logic that runs the "processor" for
            // each item "collection" with degree of parallelism "degreeOfParallelism"

            Task<R>[] tasks = null;

            List<R>[] results = null;
            return results.SelectMany(i => i).ToArray();
        }


        public static async Task ExecuteInParallelWithDegreeOfParallelism<T>(this IEnumerable<T> collection,
            Func<T, Task> processor,
            int degreeOfParallelism)
        {
            Task[] tasks = null;

            // TODO
            // Implement logic that runs the "processor" side effect for
            // each item "collection" with degree of parallelism "degreeOfParallelism"

            await Task.WhenAll(tasks);
        }

        private static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
                @this.Add(element);
        }

        public static async Task<IEnumerable<R>> ProjectInParallelWithDegreeOfParallelism<T, R>(
            this IEnumerable<T> collection,
            Func<T, Task<R>> processor,
            int degreeOfParallelism)
        {
            var results = new ConcurrentBag<R>();

            Task[] tasks = null;

            // TODO
            // Implement logic that runs the "processor" for
            // each item "collection" with degree of parallelism "degreeOfParallelism"
            // The result of each iteration is saved into local "results" queue, 
            // which is return as IEnumerable

            await Task.WhenAll(tasks);
            return results;
        }
    }
}