using System;
using System.Collections.Generic;
using System.IO;

namespace CSharpWebCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            var urls = new List<string>();
            
            urls.Add("https://www.cnn.com");
            urls.Add("https://www.bbc.com");
            urls.Add("https://www.amazon.com");
            urls.Add("https://www.cnn.com");

            DataFlowCrawler.Start(urls, async (url, buffer) =>
            {
                string fileName = Path.GetFileName(url);
                
                if (!Directory.Exists("Images"))
                    Directory.CreateDirectory("Images");
                
                string name = @"Images/" + fileName;

                using (Stream srm = File.OpenWrite(name))
                {
                    await srm.WriteAsync(buffer, 0, buffer.Length);
                }
            });

            Console.ReadLine();
        }
    }
}
