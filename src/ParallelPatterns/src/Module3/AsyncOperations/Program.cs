using SixLabors.ImageSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functional.Async;

namespace AsyncOperations
{
    class Program
    {
        private static readonly string fileDestination = "../../Data/Images";
        
        private Func<string, Task<byte[]>> DownloadSiteIcon = async domain =>
        {
            var response = await new
                HttpClient().GetAsync($"http://{domain}/favicon.ico");
            return await response.Content.ReadAsByteArrayAsync();
        };


        // Download an image(icon) from the network asynchronously
        static async Task DownloadSiteIconAsync(string domain)
        {
            using (FileStream stream = new FileStream(fileDestination,
                FileMode.Create, FileAccess.Write,
                FileShare.Write, 0x1000, FileOptions.Asynchronous))
                await new HttpClient()
                    .GetAsync($"http://{domain}/favicon.ico")
                    .Bind(async content => await
                        content.Content.ReadAsByteArrayAsync())
                    .Map(bytes => Image.Load(new MemoryStream(bytes)))
                    .Tap(image => Task.Run(() => image.Save(fileDestination)));
        }

        static async  Task DownloadSiteIconAsyncLINQ(string domain)
        {
            using (FileStream stream = new FileStream(fileDestination,
                FileMode.Create, FileAccess.Write, FileShare.Write,
                0x1000, FileOptions.Asynchronous))
                await (from response in new HttpClient()
                        .GetAsync($"http://{domain}/favicon.ico")
                    from bytes in response.Content.ReadAsByteArrayAsync()
                    select stream.WriteAsync(bytes, 0, bytes.Length));
        }
        
        static async Task ExecuteInParallelWithDegreeOfParallelism<T>(IEnumerable<T> collection,
            Func<T, Task> processor,
            int degreeOfParallelism)
        {
            // TODO 
            // Missing code here
        }


        static void Main(string[] args)
        {
            // TODO
            // control the degree of parallelism
            // use either (or both) "RequestGate" and/or "ExecuteInWithDegreeOfParallelism" class(s)
            // to be implemented 

            var urls = new List<string>
            {
                "https://edition.cnn.com", 
                "http://www.bbc.com", 
                "https://www.microsoft.com", 
                "https://www.apple.com",
                "https://www.amazon.com", 
                "https://www.facebook.com"
            };

            // TODO
            // var tasks = ExecuteInParallelWithDegreeOfParallelism(urls, DownloadSiteIconAsync, 4);
            
            var tasks = (from url in urls
                select DownloadSiteIconAsync(url))
                .ToArray();

            Task.WhenAll(tasks);
        }
    }
}
