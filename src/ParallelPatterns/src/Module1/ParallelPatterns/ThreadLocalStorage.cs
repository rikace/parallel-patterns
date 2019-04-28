using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataParallelism
{
    class ThreadLocalStorage
    {
        public void Start()
        {
            char[] delimiters = { ' ', ',', '.', ';', ':', '-', '_', '/', '\u000A' };
            var client = new WebClient();
            const string headerText = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.1; Trident/6.0)";
            client.Headers.Add("user-agent", headerText);
            var words = client.DownloadString(@"http://www.gutenberg.org/files/2009/2009.txt");
            var wordList = words.Split(delimiters, StringSplitOptions.RemoveEmptyEntries).ToList();

            //word count total
            Int32 total = 0;

            // Sting is type of source elements
            // int32 is type of thread-local count variable
            // wordlist is the source collection
            // ()=>0 initializes local variable
            Parallel.ForEach<String, Int32>(wordList, () => 0,
                (word, loopstate, count) =>  // method invoked on each iteration of loop
                    {
                        if (word.Equals("species"))
                        {
                            count++; // increment the count
                        }
                        return count;
                    }, (result) => Interlocked.Add(ref total, result)); // executed when all loops have completed

            Console.WriteLine("The word specied occured {0} times.", total);
            Console.ReadLine();
        }
    }
}
