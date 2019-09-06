using DataParallelism;
using DataParallelism.cs;
using DataParallelism.Reduce;
using Helpers;
using ParallelFilterMap;
using SixLabors.ImageSharp;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using DataParallelism.MapReduce;

namespace ParallelPatterns
{
    class Program
    {
        static void ParallelFilterMap()
        {
            bool IsPrime(int n)
            {
                if (n == 1) return false;
                if (n == 2) return true;
                var boundary = (int)Math.Floor(Math.Sqrt(n));
                for (int i = 2; i <= boundary; ++i)
                    if (n % i == 0) return false;
                return true;
            }

            BigInteger ToPow(int n) => (BigInteger)Math.BigMul(n, n);

            var numbers = Enumerable.Range(0, 100000000).ToList();

            BigInteger SeqOperation() => numbers.Where(IsPrime).Select(ToPow).Aggregate(BigInteger.Add);
            BigInteger ParallelLinqOperation() => numbers.AsParallel().Where(IsPrime).Select(ToPow).Aggregate(BigInteger.Add);

            // TODO : 6.6
            // Update the FilterMap function to reduce the result into a single (or different) type.
            // you should remove the ".Aggregate(BigInteger.Add)" part and add a reducer function.
            // Keep parallelism and thread safety
            BigInteger ParallelFilterMapInline() => numbers.FilterMap(IsPrime, ToPow).Aggregate(BigInteger.Add);

          
            Console.WriteLine("Square Prime Sum [0..10000000]");
            Func<Func<BigInteger>, Action> runSum = (func) =>
                new Action(
                    () =>
                    {
                        var result = func();
                        Console.WriteLine($"Sum = {result}");
                    });

            var sumImplementations =
                new[]
                {
                    new Tuple<String, Action>(
                        "C# Sequential", runSum(SeqOperation)),
                    new Tuple<String, Action>(
                        "C# Parallel LINQ", runSum(ParallelLinqOperation)),
                    new Tuple<String, Action>(
                        "C# Parallel FilterMap inline", runSum(ParallelFilterMapInline))
                };

            foreach (var item in sumImplementations)
            {
                BenchPerformance.Time(item.Item1, item.Item2);
            }

        }
        static void Main(string[] args)
        {

            MapReduceWordCounter.Start();
            
            // ParallelFilterMap();

            // PrimeNumbers.SumPrimeNumber_Reducer();

            
            Console.ReadLine();
        }
    }
}
