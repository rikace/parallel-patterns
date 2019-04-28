using System.Collections.Concurrent;
using HtmlAgilityPack;
using System.Linq;

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

//        private const string LINK_REGEX_HREF = "\\shref=('|\\\")?(?<LINK>http\\://.*?(?=\\1)).*>";
//        private static readonly Regex _linkRegexHRef = new Regex(LINK_REGEX_HREF);
//        private const string IMG_REGEX = "<\\s*img [^\\>]*src=('|\")?(?<IMG>http\\://.*?(?=\\1)).*>\\s*([^<]+|.*?)?\\s*</a>";
//        private static readonly Regex _imgRegex = new Regex(IMG_REGEX);

        // TODO 4
        // Use httpRgx to validate and correct url
        // why are we using "ThreadLocal" ?
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


            var downloader = new TransformBlock<string, string>(
                (url) =>
                {
                    // TODO 1
                    // implement logic to download and return the web page (from url)
                    return url;
                }, downloaderOptions);


            // TODO 2
            // implement a "printer block" to display the message
            // $"Message {DateTime.UtcNow.ToString()} - Thread ID {Thread.CurrentThread.ManagedThreadId} : {msg}"
            

            // Test 
            // this lines should be commented or removed after the test
            // UNCOMMENT: downloader.LinkTo(printer);
            foreach (var url in urls)
                downloader.Post(url);

            // this block will broadcasts the output of the "downloader" block to both the "linkParser" and "imhParser"
            var contentBroadcaster = new BroadcastBlock<string>(s => s);

            // TODO 3
            // implement a linkParser block that has as input the content of the "web page"
            // sent by "downloader" block, parses the content to extract all the anchors "a" tags
            // and the related "href", and then send out the result. 
            // keep in mind that the output can be a list of strings
            
            //var linkParser = START HERE

            // here sample code to pare the HTML content to get all the "hrer"s
            Func<string, IEnumerable<string>> parseHtml = html =>
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var links =
                    from link in doc.DocumentNode.Descendants("a")
                    where link.Attributes.Contains("href")
                    select link.GetAttributeValue("href", "");
                return links;
            };

            // TODO 4
            // implement a imgParser block that has as input the content of the "web page"
            // sent by "downloader" block, parses the content to extract all the images "img" tags
            // and the related "src", and then send out the result. 
            // keep in mind that the output can be a list of strings
            
            // reuse the same logic from the "Func<string, IEnumerable<string>> parseHtml", 
            // but adjust the html tags to target the images
            
            // var imgParser = START HERE
            
            
            var linkBroadcaster = new BroadcastBlock<string>(s => s);

            var writer = new ActionBlock<string>(async url =>
            {
                // TODO 5
                // FIX THE URL, sometimes the images have relative path
                // Download the image and run the "compute" function
                // What will happen in case of error? 
                // Can you write a defensive code to handle errors?
                
                Console.WriteLine($"Downloading {url}..");
                
            });

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

            // TODO 6
            // uncomment the code when done and run the progran
            IDisposable disposeAll = new CompositeDisposable(
                // from [downloader] to [contentBroadcaster]
                // UNCOMMENT: downloader.LinkTo(contentBroadcaster),
                // from [contentBroadcaster] to [imgParser]
                // UNCOMMENT: contentBroadcaster.LinkTo(imgParser),
                // from [contentBroadcaster] to [linkParserHRef]
                // UNCOMMENT: contentBroadcaster.LinkTo(linkParser),
                // from [linkParser] to [linkBroadcaster]
                // UNCOMMENT: linkParser.LinkTo(linkBroadcaster),
                // conditional link to from [linkBroadcaster] to [downloader]
                // UNCOMMENT: linkBroadcaster.LinkTo(downloader, linkFilter),
                // from [linkBroadcaster] to [writer]
                // UNCOMMENT: linkBroadcaster.LinkTo(writer, imgFilter),
                // from [imgParser] to [writer]
                // UNCOMMENT: imgParser.LinkTo(writer));
            );
            
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
