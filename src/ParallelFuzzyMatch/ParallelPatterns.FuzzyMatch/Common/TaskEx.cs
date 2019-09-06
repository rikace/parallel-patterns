using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ParallelPatterns.Common.TaskEx
{
    public static class TaskEx
    {
        public static Task<R[]> Traverse<T, R>(this IEnumerable<T> collection, Func<T, Task<R>> projection)
            => Task.WhenAll(collection.Select(projection).ToArray());

        public static Task<R[]> Traverse<T, R>(this IEnumerable<T> collection, Func<T, R> projection)
            => Task.FromResult(collection.Select(projection).ToArray());
        
         public static async Task<R> Apply<T, R>(this Task<Func<T, R>> f, Task<T> arg)
            => (await f.ConfigureAwait(false))(await arg.ConfigureAwait(false));

        public static Task<Func<T2, R>> Apply<T1, T2, R>(this Task<Func<T1, T2, R>> f, Task<T1> arg)
            => Apply(f.Map(Helpers.Curry), arg);

        public static Task<Func<T2, T3, R>> Apply<T1, T2, T3, R>(this Task<Func<T1, T2, T3, R>> @this, Task<T1> arg)
            => Apply(@this.Map(Helpers.Curry), arg);

        public static async Task<R> Map<T, R>(this Task<T> task, Func<T, R> f)
            => f(await task.ConfigureAwait(false));
        
        public static void FromTask<TResult, TTaskResult>(
            this TaskCompletionSource<TResult> tcs, Task<TTaskResult> task, Func<TTaskResult, TResult> resultSelector)
        {
            if (task.Status == TaskStatus.Faulted)
            {
                var ae = task.Exception;
                var targetException = ae.InnerExceptions.Count == 1 ? ae.InnerExceptions[0] : ae;
                tcs.TrySetException(targetException);
            }
            else if (task.Status == TaskStatus.Canceled)
                tcs.TrySetCanceled();
            else if (task.Status == TaskStatus.RanToCompletion)
                tcs.TrySetResult(resultSelector(task.Result));
            else
                throw new InvalidOperationException($"Task should be in one of the final states! Current state: {task.Status.ToString()}");
        }
        
        public static Task<IEnumerable<R>> TraverseA<T, R>
            (this IEnumerable<T> ts, Func<T, Task<R>> f)
            => ts.Aggregate(
                seed: Task.FromResult(Enumerable.Empty<R>()),
                func: (rs, t) => Task.FromResult(new Func<IEnumerable<R>, R, IEnumerable<R>>((xs, x) => xs.Append(x)))
                    .Apply(rs)
                    .Apply(f(t)));

        public static Task<IEnumerable<R>> TraverseA<T, R>
            (this IEnumerable<T> ts, Func<T, R> f)
            => ts.Aggregate(
                seed: Task.FromResult(Enumerable.Empty<R>()),
                func: (rs, t) => Task.FromResult(new Func<IEnumerable<R>, R, IEnumerable<R>>((xs, x) => xs.Append(x)))
                    .Apply(rs)
                    .Apply(Task.FromResult(f(t))));
    }
}