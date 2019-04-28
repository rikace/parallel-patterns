using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using ParallelPatterns;

namespace BenchmarkParallelPatterns
{
    [CoreJob(baseline: true)] //, CoreJob, CoreRtJob]
    [RPlotExporter, RankColumn]
    public class BenchmarkMonteCarlo
    {
        [Params(1000, 10000, 20000000)] public int N;

        private int iterations;

        [GlobalSetup]
        public void Setup()
        {
            iterations = N;
        }

        [Benchmark]
        public double BasicMonteCarlo() => PiMontecarlo.BaseCalculate(iterations);

        [Benchmark]
        public double MultiTasksMonteCarlo() => PiMontecarlo.MultiTasksCalculate(iterations);

        [Benchmark]
        public double ParallelForMonteCarlo() => PiMontecarlo.ParallelForCalculate(iterations);

        [Benchmark]
        public double ParallelForThreadLocalMonteCarlo() => PiMontecarlo.ParallelForThreadLocalCalculate(iterations);

        [Benchmark]
        public double PLINQMonteCarlo() => PiMontecarlo.PLINQCalculate(iterations);

        [Benchmark]
        public double PLINQPartitionerMonteCarlo() => PiMontecarlo.PLINQPartitionerCalculate(iterations);
    }



    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkMonteCarlo>();
            //BenchmarkRunner.Run<StringsWithSpan>();
        }
    }
}