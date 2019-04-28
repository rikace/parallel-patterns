using System;
using System.Collections.Immutable;
using System.Threading;

namespace ParallelPatterns.Common
{
    public sealed class Atom<T> where T : class
    {
        private volatile T value;
        public Atom(T value)         {
            this.value = value;
        }

        public T Swap(Func<T, T> operation)
        {
            var original = value;
            var temp = operation(original);
#pragma warning disable 420
            if (Interlocked.CompareExchange(ref value, temp, original) != original)
#pragma warning restore 420
            {
                var spinner = new SpinWait();
                do
                {
                    spinner.SpinOnce();
                    original = value;
                    temp = operation(original);
                }
#pragma warning disable 420
                while (Interlocked.CompareExchange(ref value, temp, original) != original);
#pragma warning restore 420
            }
            return original;
        }
    }
    
    public sealed class AtomImmutable<T>
    {
        public ImmutableArray<T> Value {get;}
        public AtomImmutable(ImmutableArray<T> value)
        {
            this.Value = value;
        }

        public ImmutableArray<T> Swap(Func<ImmutableArray<T>, ImmutableArray<T>> operation)
        {
            var original = Value;
            var temp = operation(original);
            if (ImmutableInterlocked.InterlockedCompareExchange<T>(ref original, temp, Value) != original)
            {
                var spinner = new SpinWait();
                do
                {
                    spinner.SpinOnce();
                    original = Value;
                    temp = operation(original);
                }
                while (ImmutableInterlocked.InterlockedCompareExchange<T>(ref original, temp, Value) != original);
            }
            return original;
        }
    }

}