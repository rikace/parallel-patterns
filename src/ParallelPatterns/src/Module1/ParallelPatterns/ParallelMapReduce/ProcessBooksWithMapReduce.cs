using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CommonHelpers;
using Helpers;
using Newtonsoft.Json;

namespace DataParallelism
{
   public class ProcessBooksWithMapReduce
    {
        private static char[] delimiters = { ' ', ',', ';', ':', '\"', '.' };
        static string ProcessBookAsync(string bookContent, string title, HashSet<string> stopwords)
        {
            using (var reader = new StringReader(bookContent))
            {
                // TODO : 2.5
                // (1)
                // Apply the Map Reduce pattern to calculate the unique words in the book
                // the return type should contain a pair of values, to map each word with the number of appearances in the book
                // is the following function a pure function?  (using the "reader") 
                IEnumerable<(string, int)> words = default(IEnumerable<(string, int)>);
                // Suggestion : start looking into >> reader.EnumLines().AsParallel();

                var sb = new StringBuilder();

                sb.AppendLine($"'{title}' book stats");
                sb.AppendLine("Top ten words used in this book: ");

                // (2)
                // uncomment this code
                //foreach (var w in words.Take(10))
                //    sb.AppendLine($"Word: '{w.Word}', times used: '{w.Count}'");

                sb.AppendLine($"Unique Words used: {words.Count()}");

                return sb.ToString();
            }
        }

        async static Task<string> DownloadBookAsync(string bookUrl)
        {
            using (var client = new HttpClient())
                return await client.GetStringAsync(bookUrl);
        }

        async static Task<HashSet<string>> DownloadStopWordsAsync()
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

        public static void RunBookMapReduce()
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

                string result = ProcessBookAsync(bookContent, key, stopwords);

                output.Append(result);
                output.AppendLine();
            });

            Console.Write(output.ToString());
        }

        public static void Run()
        {
            BenchPerformance.Time("Parallel Map Reduce word count", RunBookMapReduce, 5);
        }
    }
}
