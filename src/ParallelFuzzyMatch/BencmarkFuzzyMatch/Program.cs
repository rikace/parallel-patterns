using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using ParallelPatterns;

namespace BenchmarkFuzzyMatch
{
    [CoreJob(baseline: true)] 
    [RPlotExporter, RankColumn]
    public class BenchmarkFuzzyMatch
    {
        private static readonly string[] WordsToSearch =
            {"ENGLISH", "RICHARD", "STEALING", "MAGIC", "STARS", "MOON", "CASTLE"};

        private IList<string> files;

        [GlobalSetup]
        public void Setup()
        {
            files =
                Directory.EnumerateFiles("../Data", "*.txt")
                    .Select(f => new FileInfo(f))
                    .OrderBy(f => f.Length)
                    .Select(f => f.FullName)
                    .Take(1).ToList();
        }

        [Benchmark]
        public void Sequential()
        {
            ParallelFuzzyMatch.RunFuzzyMatchSequential(WordsToSearch, files);
        }

        [Benchmark]
        public void TaskContinuation()
        {
            ParallelFuzzyMatch.RunFuzzyMatchTaskContinuation(WordsToSearch, files).Wait();
        }
     

        [Benchmark]
        public void TaskComposition()
        {
            ParallelFuzzyMatch.RunFuzzyMatchTaskComposition(WordsToSearch, files).Wait();
        }

        [Benchmark]
        public void TaskLINQ()
        {
            ParallelFuzzyMatch.RunFuzzyMatchTaskLINQ(WordsToSearch, files).Wait();
        }
        
        [Benchmark]
        public void Pipeline()
        {
            ParallelFuzzyMatch.RunFuzzyMatchPipelineCSharp(WordsToSearch, files);
        }
        
        [Benchmark]
        public void ProcessTasksAsCompleteBasic()
        {
            ParallelFuzzyMatch.RunFuzzyMatchTaskProcessAsCompleteBasic(WordsToSearch, files).Wait();
        }
        
        [Benchmark]
        public void BetterTaskContinuation()
        {
            ParallelFuzzyMatch.RunFuzzyMatchBetterTaskContinuation(WordsToSearch, files).Wait();
        }

        [Benchmark]
        public void FuzzyMatchDataFlow()
        {
            ParallelFuzzyMatch.RunFuzzyMatchDataFlow(WordsToSearch, files).Wait();
        }

    }


    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkFuzzyMatch>(new DebugBuildConfig());
           
           
           Console.ReadLine();

        }
    }
}