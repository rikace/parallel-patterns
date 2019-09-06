using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

namespace Helpers
{
    // Atom object to perform CAS instruction
    public class Atom<T> where T : class //#A
    {
        public Atom(T value)
        {
            this.value = value;
        }

        protected volatile T value;

        public T Value => value; //#B

        public virtual T Swap(Func<T, T> operation) //#C
        {
            T original, temp;
            do
            {
                original = value;
                temp = operation(original);
            }
#pragma warning disable 420
            while (Interlocked.CompareExchange(ref value, temp, original) != original); //#D
#pragma warning restore 420
            return original;
        }
    }

    public sealed class AtomOptimized<T> : Atom<T> where T : class
    {
        public AtomOptimized(T value) : base(value) { }

        public override T Swap(Func<T, T> operation)
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
        public ImmutableArray<T> Value { get; }
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
