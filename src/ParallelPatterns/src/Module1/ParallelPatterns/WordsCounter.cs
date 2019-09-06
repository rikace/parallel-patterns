using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace DataParallelism.CSharp
{
    // TODO : 2.3
    // Replace repetitive code with the MapReduce extension method
    // TODO : 2.3.2 Decouple and isolate side effects
    public static class WordsCounterDemo
    {
        //  Parallel Words Counter program with side effects
        public static Dictionary<string, int> WordsCounter(string source)
        {
            var wordsCount =
                    (from filePath in
                        Directory.GetFiles(source, "*.txt")
                                 .AsParallel()
                     from line in File.ReadLines(filePath)
                     from word in line.Split(' ')
                     select word.ToUpper())
                .GroupBy(w => w)
                .OrderByDescending(v => v.Count()).Take(10);
            return wordsCount.ToDictionary(k => k.Key, v => v.Count());
        }


        // TODO Decouple and isolate side effects
        // make "WordsPureCounter" pure

        private static Dictionary<string, int> WordsUnpureCounter(string source)
        {
            var wordsCount =
                    (from filePath in
                        Directory.GetFiles(source, "*.txt")
                                 .AsParallel()
                     from line in File.ReadLines(filePath)
                     from word in line.Split(' ')
                     select word.ToUpper())
                .GroupBy(w => w)
                .OrderByDescending(v => v.Count()).Take(10);
            return wordsCount.ToDictionary(k => k.Key, v => v.Count());
        }
        
        public static void Run()
        {
            var dataPath = @"Shakespeare";

            Func<Func<string, Dictionary<string, int>>, Action[]> run = (func) =>
                    new Action[] { () => { func(dataPath); } };

            var implementations =
                new[]
                {
                    new Tuple<String, Action[]>(
                        "WordCounter", run(WordsCounter)),
                    new Tuple<String, Action[]>(
                        "Pure WordCounter", run(WordsUnpureCounter))
                };
        }
    }
}


