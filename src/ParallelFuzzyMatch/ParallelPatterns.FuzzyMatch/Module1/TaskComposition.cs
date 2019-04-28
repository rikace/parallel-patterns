using System;
using System.Threading.Tasks;

namespace ParallelPatterns.TaskComposition
{
    public static class TaskComposition
    {
        // TODO (1)
        // implement missing code
        public static Task<TOut> Then<TIn, TOut>(
            this Task<TIn> task,
            Func<TIn, TOut> next)
        {
            var tcs = new TaskCompletionSource<TOut>();

            // Missing code

            return tcs.Task;
        }

        // TODO (1)
        // implement missing code
        public static Task<TOut> Then<TIn, TOut>(
            this Task<TIn> task,
            Func<TIn, Task<TOut>> next)
        {
            var tcs = new TaskCompletionSource<TOut>();

            // Missing code

            return tcs.Task;
        }


        public static Task<TOut> Select<TIn, TOut>(
            this Task<TIn> task,
            Func<TIn, TOut> projection)
        {
            var r = new TaskCompletionSource<TOut>();

            // Missing code

            return r.Task;
        }
    }
}
