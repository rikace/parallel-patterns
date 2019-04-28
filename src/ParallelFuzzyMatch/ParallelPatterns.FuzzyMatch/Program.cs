using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ParallelPatterns
{
    public static class Program
    {
        private static readonly string[] WordsToSearch =
            {"ENGLISH", "RICHARD", "STEALLING", "MAGIC", "STARS", "MOON", "CASTLE"};


        private static async Task Start(IList<string> files)
        {
            Console.WriteLine(@"=============================================================
Press the number of the method you want to run and press ENTER

(1) RunFuzzyMatchc Task Composition  (TODO 1)
(2) RunFuzzyMatch Task LINQ  (TODO 2)
(3) RunFuzzyMatchc Pipeline  (TODO 3)
(4) RunFuzzyMatch Process Tasks as complete (TODO 4)
(5) RunFuzzyMatchDataFlow  (TODO 5 - 6)
(6) RunFuzzyMatch Agent C# (TODO 7 C#)
(7) RunFuzzyMatch Agent F# (TODO 7 F#)
(8) Agent Aggregate (TODO 8 C#)
=============================================================
");

            var choice = Console.ReadLine();
            var indexChoice = int.Parse(choice);
            var watch = Stopwatch.StartNew();

            switch (indexChoice)
            {
                case 1:
                    // TODO 1
                    await ParallelFuzzyMatch.RunFuzzyMatchTaskComposition(WordsToSearch, files);

                    break;
                case 2:
                    // TODO 2
                    await ParallelFuzzyMatch.RunFuzzyMatchTaskLINQ(WordsToSearch, files);

                    break;
                case 3:
                    // TODO 3
#if FSHARP
                    ParallelFuzzyMatch.RunFuzzyMatchPipelineFSharp(WordsToSearch, files);
#else
ParallelFuzzyMatch.RunFuzzyMatchPipelineCSharp(WordsToSearch, files);
#endif
                    
                    break;
                case 4:
                    // TODO 4
                    await ParallelFuzzyMatch.RunFuzzyMatchTaskProcessAsCompleteAbstracted(WordsToSearch, files);

                    break;
                case 5:
                    // TODO 5 - 6
                    await ParallelFuzzyMatch.RunFuzzyMatchDataFlow(WordsToSearch, files);

                    break;
                case 6:
                    // TODO 7 (C#)
                    await ParallelFuzzyMatch.RunFuzzyMatchAgentCSharp(WordsToSearch, files);

                    break;
                case 7:
                    // TODO 7 (F#)
                    await ParallelFuzzyMatch.RunFuzzyMatchAgentFSharp(WordsToSearch, files);

                    break;
                case 8:
                    // TODO 8 (C#)
                    AgentAggregate.Run();

                    break;
                default:
                    throw new Exception("Selection not supported");
            }
            watch.Stop();

            Console.WriteLine($"<< DONE in {watch.Elapsed.ToString()} >>");
        }

        static async Task Main(string[] args)
        {
            IList<string> files =
                    Directory.EnumerateFiles("./Data/Text", "*.txt")
                        .Select(f => new FileInfo(f))
                        .OrderBy(f => f.Length)
                        .Select(f => f.FullName)
                        .Take(5).ToList();

            await Start(files);
            Console.ReadLine();
        }
    }
}
            