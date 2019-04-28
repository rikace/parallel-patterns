using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AsyncOperations;

namespace AsyncOperations
{
    class Program
    {
        static void Main(string[] args)
        {
            var urls = new List<string> { "https://edition.cnn.com", "http://www.bbc.com", "https://www.microsoft.com" };

            var asyncOps = new AsyncOperations();

            var tasks = (from url in urls
                         select asyncOps.DownloadIconAsync(url, "../../../../../Data/Images")).ToArray();

            Task.WhenAll(tasks);
        }
    }
}
