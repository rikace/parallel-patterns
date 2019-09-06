using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using FuzzyMatch;

namespace ParallelPatterns.Common
{
    public static class FuzzyMatchHelpers
    {
        public const double threshold = 0.9;
        
        public static readonly HashSet<string> IgnoreWords =
            new HashSet<string>() {"a", "an", "the", "and", "of", "to"};

        public static readonly ThreadLocal<Regex> WordRegex =
            new ThreadLocal<Regex>(() => new Regex(@"((\b[^\s]+\b)((?<=\.\w).)?)", RegexOptions.Compiled));

        public static Lazy<char[]> punctuation =
            new Lazy<char[]>(() =>
                Enumerable.Range(0, 256).Select(c => (char) c).Where(c => Char.IsWhiteSpace(c) || Char.IsPunctuation(c))
                    .ToArray());

        public static Dictionary<string, HashSet<string>> Clone(Dictionary<string, HashSet<string>> state)
            => state.ToDictionary(x => x.Key, x => new HashSet<string>(x.Value));


        public static IDictionary<string, HashSet<string>> PrintSummary(
            HashSet<JaroWinklerModule.FuzyMatchStructures.WordDistanceStruct> summaryMathces)
        {
            var matchesDic = summaryMathces
                .GroupBy(w => w.Word)
                .ToDictionary(
                    k => k.Key,
                    v => v.Select(w => w.Match).AsSet());
            return PrintSummary(matchesDic);
        }


        public static IDictionary<string, HashSet<string>> PrintSummary(
            IDictionary<string, HashSet<string>> summaryMathces)
        {
            foreach (var sm in summaryMathces)
            {
                Console.WriteLine(new string('=', 25));

                Console.WriteLine($"Matches for {sm.Key}");
                foreach (var m in sm.Value.Take(5))
                    Console.WriteLine($"\t{m}");

                Console.WriteLine(new string('=', 25));
            }

            return summaryMathces;
        }

    }
}