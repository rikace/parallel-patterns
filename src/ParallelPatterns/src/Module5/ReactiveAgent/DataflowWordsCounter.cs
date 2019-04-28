using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using System.Net;

namespace DataflowPipeline
{
    public class DataflowPipeline
    {
        // TODO : 5.6
        // convert into producer/consumer word counter with agent
        // then convert any step with RX for testing
        public static void Start()
        {
            const int bc = 1;
            var opt = new ExecutionDataflowBlockOptions()
            {
                BoundedCapacity = bc,
                MaxDegreeOfParallelism = 1

            };
            // Download a book as a string
            var downloadBook = new Func<string, string>(uri =>
            {
                Console.WriteLine("Downloading the book...");

                return new WebClient().DownloadString(uri);

            });

            // splits text into an array of strings.
            var createWordList = new Func<string, string[]>(text =>
            {
                Console.WriteLine("Creating list of words...");

                // Remove punctuation
                char[] tokens = text.ToArray();
                for (int i = 0; i < tokens.Length; i++)
                {
                    if (!char.IsLetter(tokens[i]))
                        tokens[i] = ' ';
                }
                text = new string(tokens);

                return text.Split(new char[] { ' ' },
                   StringSplitOptions.RemoveEmptyEntries);
            });

            // Remove short words and return the count
            var filterWordList = new Func<string[], int>(words =>
            {
                Console.WriteLine("Counting words...");

                var wordList = words.Where(word => word.Length > 3).OrderBy(word => word)
                   .Distinct().ToArray();
                return wordList.Count();
            });

            // Implement an agent named "printWordCount" block that print the results
            // then link the pipeline 

            var printWordCount = new Action<int>(wordcount =>
            {
                Console.WriteLine("Found {0} words",
                   wordcount);
            });

            // TODO : 5.6
            // implement the Dataflow building blocks using the previous 
            // generic delegates.
            // Then link the blocks to generate the pipeline
            // The last step of the pipeline should be using Reactive Extension.

            // TODO uncomment these code and start the data-flow 
            // Download Origin of Species
            // downloadBookBlock.Post("http://www.gutenberg.org/files/2009/2009.txt");
            // downloadBookBlock.Post("http://www.gutenberg.org/files/2010/2010.txt");
            // downloadBookBlock.Post("http://www.gutenberg.org/files/2011/2011.txt");

            // TODO
            // each completion task in the pipeline creates a continuation task
            // that marks the next block in the pipeline as completed.
            // A completed dataflow block processes any buffered elements, but does
            // not accept new elements.

            // TODO print the 10 most popular words 
            // how do you keep the state ?
            
            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();

        }
    }
}
