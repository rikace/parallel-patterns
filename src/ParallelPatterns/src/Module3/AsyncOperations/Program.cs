using SixLabors.ImageSharp;
using System;
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
        private Func<string, Task<byte[]>> DownloadSiteIcon = async domain =>
        {
            var response = await new
                HttpClient().GetAsync($"http://{domain}/favicon.ico");
            return await response.Content.ReadAsByteArrayAsync();
        };


        // Download an image(icon) from the network asynchronously
        public async Task DownloadSiteIconAsync(string domain, string fileDestination)
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

        async Task DownloadSiteIconAsyncLINQ(string domain, string fileDestination)
        {
            using (FileStream stream = new FileStream(fileDestination,
                FileMode.Create, FileAccess.Write, FileShare.Write,
                0x1000, FileOptions.Asynchronous))
                await (from response in new HttpClient()
                        .GetAsync($"http://{domain}/favicon.ico")
                    from bytes in response.Content.ReadAsByteArrayAsync()
                    select stream.WriteAsync(bytes, 0, bytes.Length));
        }

        static void Main(string[] args)
        {
            // TODO
            // control the degree of parallelism
            // use either (or both) "RequestGate" and/or "ExecuteInWithDegreeOfParallelism" class(s)
            // to be implemented 

            var urls = new List<string>
            {
                "https://edition.cnn.com", "http://www.bbc.com", "https://www.microsoft.com", "https://www.apple.com",
                "https://www.amazon.com", "https://www.facebook.com"
            };

            var asyncOps = new Program();

            var tasks = (from url in urls
                select asyncOps.DownloadSiteIconAsync(url, "../../Data/Images")).ToArray();

            Task.WhenAll(tasks);
        }
    }
}