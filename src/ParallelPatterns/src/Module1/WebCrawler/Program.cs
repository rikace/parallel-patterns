using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Threading;

namespace WebCrawler
{
    class Program
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
        // can you replace it using Memoization ?
        async static Task Crawler()
        {
            // code here
        }

        // --------------------------------------------------------------
        // Start 100 of web crawlers using only small number of threads
        static void WebCrawelerProducerConsumer(List<string> urls )
        {
            foreach (var url in urls)
                pending.Add(url);

            pending.Add("http://www.cnn.com");
            for (int i = 0; i < 10; i++)
                Crawler();
        }


        static void Main(string[] args)
        {

            List<string> urls = new List<string> {
                @"http://www.google.com",
                @"http://www.microsoft.com",
                @"http://www.bing.com",
                @"http://www.google.com"
            };


            WebCrawelerProducerConsumer(urls);

            WebCrawlerExample.RunDemo(urls);

            Console.ReadLine();
        }
    }
}
