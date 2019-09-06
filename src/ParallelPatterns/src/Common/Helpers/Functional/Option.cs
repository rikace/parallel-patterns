using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ParallelPatterns.Common
{
    using FunctionalHelpers;
    using Microsoft.FSharp.Core;
    using static OptionHelpers;

    public static class OptionHelpers
    {
        public static Option<T> Some<T>(T value) => new Option.Some<T>(value);
        public static Option.None None => Option.None.Default;
    }

    public struct Option<T> : IEquatable<Option.None>, IEquatable<Option<T>>
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Option<T> && Equals((Option<T>) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (EqualityComparer<T>.Default.GetHashCode(Value) * 397) ^ isSome.GetHashCode();
            }
        }

        public readonly T Value;
        readonly bool isSome;
        bool isNone => !isSome;

        private Option(T value)
        {
            if (value == null)
                throw new ArgumentNullException();
            this.isSome = true;
            this.Value = value;
        }

        public static implicit operator Option<T>(Option.None _) => new Option<T>();
        public static implicit operator Option<T>(Option.Some<T> some) => new Option<T>(some.Value);

        public static implicit operator Option<T>(T value)
            => value == null ? None : Some(value);

        public R Match<R>(Func<R> none, Func<T, R> some)
            => !isSome ? none() : some(Value);

        public bool Equals(Option<T> other)
            => this.isSome == other.isSome
               && (this.isNone || this.Value.Equals(other.Value));

        public bool Equals(Option.None _) => isNone;

        public static bool operator ==(Option<T> @this, Option<T> other) => @this.Equals(other);
        public static bool operator !=(Option<T> @this, Option<T> other) => !(@this == other);
    }

    namespace Option
    {
        public struct None
        {
            internal static readonly None Default = new None();
        }

        public struct Some<T>
        {
            internal T Value { get; }

            internal Some(T value)
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                Value = value;
            }
        }
    }

    public static class OptionExt
    {
        public static Option<T> ToOption<T>(FSharpOption<T> fsOption) =>
            FSharpOption<T>.get_IsSome(fsOption)
                ? Some(fsOption.Value)
                : None;

        public static FSharpOption<T> ToFsOption<T>(Option<T> option) =>
            option.Match(() => FSharpOption<T>.None,
                v => FSharpOption<T>.Some(v));

        public static Option<R> Map<T, R>
            (this Option.None _, Func<T, R> f)
            => None;

        public static Option<R> Map<T, R>
            (this Option.Some<T> some, Func<T, R> f)
            => Some(f(some.Value));

        public static Option<R> Map<T, R>
            (this Option<T> optT, Func<T, R> f)
            => optT.Match(
                () => None,
                (t) => Some(f(t)));

        public static Option<Func<T2, R>> Map<T1, T2, R>
            (this Option<T1> @this, Func<T1, T2, R> func)
            => @this.Map(func.Curry());

        public static Option<Func<T2, T3, R>> Map<T1, T2, T3, R>
            (this Option<T1> @this, Func<T1, T2, T3, R> func)
            => @this.Map(func.Curry());


        public static bool IsSome<T>(this Option<T> @this)
            => @this.Match(
                () => false,
                (_) => true);

        internal static T ValueUnsafe<T>(this Option<T> @this)
            => @this.Match(
                () => { throw new InvalidOperationException(); },
                (t) => t);
    }
}