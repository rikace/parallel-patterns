using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataParallelism.MapReduce
{
    public static class MapReduceWordCounter
    {
       public static void Start()
        {
            var booksList = new Dictionary<string, string>()
            {
                ["Moby Dick; Or, The Whale by Herman Melville"]
                = "http://www.gutenberg.org/cache/epub/2701/pg2701.txt",

                ["The Adventures of Tom Sawyer by Mark Twain"]
                = "http://www.gutenberg.org/cache/epub/74/pg74.txt",

                ["Treasure Island by Robert Louis Stevenson"]
                = "http://www.gutenberg.org/cache/epub/120/pg120.txt",

                ["The Picture of Dorian Gray by Oscar Wilde"]
                = "http://www.gutenberg.org/cache/epub/174/pg174.txt"
            };

            HashSet<string> stopwords = DownloadStopWordsAsync().GetAwaiter().GetResult();

            
            // TODO : 2.7
            // Refactor the Parallel loop
            // The string builder should be initialized inside the loop and aggregate
            // when the operation completes
            //
            // You could replace the Parallel Loop with PLINQ
            var output = new StringBuilder();
            Parallel.ForEach(booksList.Keys, key =>
            {
                var bookContent = DownloadBookAsync(booksList[key])
                    .GetAwaiter().GetResult();

                string result = ProcessBookAsync(bookContent, key, stopwords)
                    .GetAwaiter().GetResult();

                output.Append(result);
                output.AppendLine();
            });

            Console.Write(output.ToString());
            Console.ReadLine();
        }

        static readonly char[] delimiters = { ' ', ',', ';', ':', '\"', '.' };

        static async Task<string> ProcessBookAsync(
            string bookContent, string title, HashSet<string> stopwords)
        {
            using (var reader = new StringReader(bookContent))
            {
                var query = reader.EnumLines()
                    .AsParallel()   // TODO : implement the map-reduce also removing this parallel option
                    .SelectMany(line => line.Split(delimiters))
                    // TODO : complete the map-reduce function
                    //        implement both cases with and without ParallelQuery as argument
                    .MapReduce(
                        word => new[] { word.ToLower() },
                        key => key,
                        g => new[] { new { Word = g.Key, Count = g.Count() } }
                    )
                    .ToList();
                    
                    // TODO (extra)
                    // can you filter inside a MapReduce? 
                    // if so, where would inject the filter step and how?
                    // Look into the file "ParallelFilerMap.cs"
                        
                var words = query
                    .Where(element =>
                        !string.IsNullOrWhiteSpace(element.Word)
                        && !stopwords.Contains(element.Word))
                    .OrderByDescending(element => element.Count);

                var sb = new StringBuilder();

                sb.AppendLine($"'{title}' book stats");
                sb.AppendLine("Top ten words used in this book: ");
                foreach (var w in words.Take(10))
                {
                    sb.AppendLine($"Word: '{w.Word}', times used: '{w.Count}'");
                }

                sb.AppendLine($"Unique Words used: {query.Count()}");

                return sb.ToString();
            }
        }

        static async Task<string> DownloadBookAsync(string bookUrl)
        {
            using (var client = new HttpClient())
            {
                return await client.GetStringAsync(bookUrl);
            }
        }

        static async Task<HashSet<string>> DownloadStopWordsAsync()
        {
            string url =
                "https://raw.githubusercontent.com/6/stopwords/master/stopwords-all.json";

            using (var client = new HttpClient())
            {
                try
                {
                    var content = await client.GetStringAsync(url);
                    var words =
                        JsonConvert.DeserializeObject
                            <Dictionary<string, string[]>>(content);
                    return new HashSet<string>(words["en"]);
                }
                catch
                {
                    return new HashSet<string>();
                }
            }
        }
    }

    static class Extensions
    {
        public static IEnumerable<string> EnumLines(this StringReader reader)
        {
            while (true)
            {
                string line = reader.ReadLine();
                if (null == line) yield break;

                yield return line;
            }
        }
    }
}
