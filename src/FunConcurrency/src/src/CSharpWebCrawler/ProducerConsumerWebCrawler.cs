namespace WebCrawler
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using HtmlAgilityPack;
    using System.Net;
    using System.Threading.Tasks;
    using System.Collections.Concurrent;
    using System.Threading;

    public class ProducerConsumerWebCrawler
    {
        static IEnumerable<string> ExtractLinks(HtmlDocument doc)
        {
            try
            {
                return
                    (from a in doc.DocumentNode.SelectNodes("//a")
                        where a.Attributes.Contains("href")
                        let href = a.Attributes["href"].Value
                        where href.StartsWith("http://")
                        let endl = Math.Min(href.IndexOf('?'), href.IndexOf('#'))
                        select endl > 0 ? href.Substring(0, endl) : href).ToArray();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        static string GetTitle(HtmlDocument doc)
        {
            try
            {
                var title = doc.DocumentNode.SelectSingleNode("//title");
                return title != null ? title.InnerText.Trim() : "Untitled";
            }
            catch
            {
                return "Untitled";
            }
        }

        static async Task<HtmlDocument> DownloadDocument(string url)
        {
            try
            {
                var wc = new WebClient();
                var html = await wc.DownloadStringTaskAsync(new Uri(url));
                var doc = new HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
            catch
            {
                return new HtmlDocument();
            }
        }

        // Crawler process that adds URLs to be crawled to 'pending'
        // and keeps a list of visited URLs in 'visited'

        static BlockingCollection<string> pending = new BlockingCollection<string>();
        static ConcurrentDictionary<string, bool> visited = new ConcurrentDictionary<string, bool>();

        // TODO : 1.1
        // (1) implement a web-crawler
        // leverage the pending & visited collections to avoid to download the same site twice
        // can you leverage Memoization ?
        async static Task Crawler()
        {
            // code here
            // check the API of the "BlockingCollection" to get the an 
            // item safely 
            
            // Use the Memoize function to check if a web page has been already visited
        }

        // --------------------------------------------------------------
        // Start 100 web-crawlers concurrent using only small number of threads
        static void WebCrawlerProducerConsumer(List<string> urls)
        {
            foreach (var url in urls)
                pending.Add(url);

            pending.Add("https://www.cnn.com");
            pending.Add("https://www.foxnews.com");
            pending.Add("https://www.amazon.com");
            pending.Add("https://www.cnn.com");

            // TODO how can I lunch 10 concurrent Crawler?
            for (int i = 0; i < 10; i++)
                Crawler();
        }
    }
}
