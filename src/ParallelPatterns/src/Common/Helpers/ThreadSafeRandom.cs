using System;
using System.Threading;

namespace Helpers
{
    public sealed class ThreadSafeRandom : Random
    {
        private ThreadLocal<Random> random =
            new ThreadLocal<Random>(() => new Random(MakeRandomSeed()));

        public override int Next() => random.Value.Next();

        public override int Next(int maxValue) => random.Value.Next(maxValue);

        public override int Next(int minValue, int maxValue) => random.Value.Next(minValue, maxValue);

        public override double NextDouble() => random.Value.NextDouble();

        public override void NextBytes(byte[] buffer) => random.Value.NextBytes(buffer);

        // creates a seed that does not depend on the system-clock.
        // a unique value is created with each invocation
        static int MakeRandomSeed() => Guid.NewGuid().ToString().GetHashCode();
    }
}