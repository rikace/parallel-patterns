using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Linq;
using WebCrawler;

namespace CSharpWebCrawler
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Reactive.Disposables;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using System.Text.RegularExpressions;

    public static class DataFlowCrawler
    {
        private static ConsoleColor[] colors = new ConsoleColor[]
        {
            ConsoleColor.Black,
            ConsoleColor.DarkBlue,
            ConsoleColor.DarkGreen,
            ConsoleColor.DarkCyan,
            ConsoleColor.DarkRed,
            ConsoleColor.DarkMagenta,
            ConsoleColor.DarkYellow,
            ConsoleColor.Gray,
            ConsoleColor.DarkGray,
            ConsoleColor.Blue,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Red,
            ConsoleColor.Magenta,
            ConsoleColor.Yellow,
            ConsoleColor.White
        };

        static int index = 0;
        static ConcurrentDictionary<int, ConsoleColor> mapColors = new ConcurrentDictionary<int, ConsoleColor>();

        private static ConsoleColor ColorByInt(int id)
            => mapColors.GetOrAdd(id, _ => colors[Interlocked.Increment(ref index) % (colors.Length - 1)]);


        private static void WriteLineInColor(string message, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(message);
            Console.ResetColor();
        }
   private const string LINK_REGEX_HREF = "\\shref=('|\\\")?(?<LINK>http\\://.*?(?=\\1)).*>";
        private static readonly Regex _linkRegexHRef = new Regex(LINK_REGEX_HREF);
        private const string IMG_REGEX = "<\\s*img [^\\>]*src=('|\")?(?<IMG>http\\://.*?(?=\\1)).*>\\s*([^<]+|.*?)?\\s*</a>";
        private static readonly Regex _imgRegex = new Regex(IMG_REGEX);

        private static ThreadLocal<Regex> httpRgx = new ThreadLocal<Regex>(() => new Regex(@"^(http|https|www)://.*$"));
        public static IDisposable Start(List<string> urls, Func<string, byte[], Task> compute)
        {
            // TODO 2 
            // in order, try to increase the level of parallelism, 
            // and then implement a way to cancel the operation.
            // Maybe first try with a time expiration, then add cancellation semantic 
            var downloaderOptions = new ExecutionDataflowBlockOptions()
            {
            };

            var downloadUrl = Memoization.MemoizeLazyThreadSafe<string, string>(async (url) =>
            {
                // TODO 1
                // implement a download web page
                using (WebClient wc = new WebClient())
                {
                    string result = await wc.DownloadStringTaskAsync(url);
                    return result;
                }
            });
            
            
            var downloader = new TransformBlock<string, string>(downloadUrl
                , downloaderOptions);


            // TODO 1
            // implement a "printer block" to display the message
            var printer = new ActionBlock<string>(msg =>
                {
                    Console.WriteLine(
                        $"Message {DateTime.UtcNow.ToString()} - Thread ID {Thread.CurrentThread.ManagedThreadId} : {msg}");
                });

//            // Test 
             // downloader.LinkTo(printer);
//            foreach (var url in urls)
//            {
//                downloader.Post(url);
//            }

         
            var contentBroadcaster = new BroadcastBlock<string>(s => s);

            var linkParser = new TransformManyBlock<string, string>(
                (html) =>
                {
                    var output = new List<string>();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var links =
                        from link in doc.DocumentNode.Descendants("a")
                        where link.Attributes.Contains("href")
                        select link.GetAttributeValue("href", "");

//                    var linksValidated =
//                        from link in links
//                        where httpRgx.Value.IsMatch(link)
//                        select link;
//                        
                    foreach (var link in links)
                    {
                        Console.WriteLine($"Link {link} ready to be crawled");
                        output.Add(link);
                    }

                    return output;
                });

            var imgParser = new TransformManyBlock<string, string>(
                (html) =>
                {
                    var output = new List<string>();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var images =
                        from img in doc.DocumentNode.Descendants("img")
                        where img.Attributes.Contains("src")
                        select img.GetAttributeValue("src", "");

//                    var imagesValidated =
//                        from img in images
//                        where httpRgx.Value.IsMatch(img)
//                        select img;
               
                    foreach (var img in images)
                    {
                        Console.WriteLine($"image {img} ready to be downloaded");
                        output.Add(img);
                    }


                    return output;
                });


            // Step 3

            var linkBroadcaster = new BroadcastBlock<string>(s => s);

            var writer = new ActionBlock<string>(async url =>
            {
                url = url.StartsWith("http") ? url : "http:" + url;
                using (WebClient wc = new WebClient())
                {
                    // using IOCP the thread pool worker thread does return to the pool
                    byte[] buffer = await wc.DownloadDataTaskAsync(url);
                    Console.WriteLine($"Downloading {url}..");
                    await compute(url, buffer);
                }
            });

          // linkBroadcaster.AsObservable().
                
            StringComparison comparison = StringComparison.InvariantCultureIgnoreCase;
            Predicate<string> linkFilter = link =>
                link.IndexOf(".aspx", comparison) != -1 ||
                link.IndexOf(".php", comparison) != -1 ||
                link.IndexOf(".htm", comparison) != -1 ||
                link.IndexOf(".html", comparison) != -1;

            Predicate<string> imgFilter = url =>
                url.EndsWith(".jpg", comparison) ||
                url.EndsWith(".png", comparison) ||
                url.EndsWith(".gif", comparison);

            IDisposable disposeAll = new CompositeDisposable(
                // from [downloader] to [contentBroadcaster]
                downloader.LinkTo(contentBroadcaster),
                // from [contentBroadcaster] to [imgParser]
                contentBroadcaster.LinkTo(imgParser),
                // from [contentBroadcaster] to [linkParserHRef]
                contentBroadcaster.LinkTo(linkParser),
                // from [linkParser] to [linkBroadcaster]
                linkParser.LinkTo(linkBroadcaster),
                // conditional link to from [linkBroadcaster] to [downloader]
                linkBroadcaster.LinkTo(downloader, linkFilter),
                // from [linkBroadcaster] to [writer]
                linkBroadcaster.LinkTo(writer, imgFilter),
                // from [imgParser] to [writer]
                imgParser.LinkTo(writer));
            
            // TODO 7
            // Use Reactive Extension to output some colorful logging
            
            // TODO 8 
            // Can you memoize the blocks to avoid to re-parse duplicates URLs ??
            
            // TODO 9 
            // Write a batch block to parse group of 2 images per operation
            // Where should you link this block?
            // how do you ensure that all the images are parse also in case of Odd numbers?
            
            foreach (var url in urls)
            {
                downloader.Post(url);
            }

            return disposeAll;
        }
    }
}
