using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Pipeline
{
    public class ProducerConsumer
    {
        public BlockingCollection<string> inputData = null;
        public BlockingCollection<Image<Rgba32>> outputData = null;
        
        Func<string, Task<Image<Rgba32>>> function = null;
        
        public async Task Stage1()
        {
            Console.WriteLine($"Stage - is running with Thread ID #{Thread.CurrentThread.ManagedThreadId}");
            while (!inputData.IsCompleted)
            {
                if (inputData.TryTake(out var receivedItem, 50))
                {
                    if (outputData != null)
                    {
                        var outputItem = await function(receivedItem);
                        outputData.TryAdd(outputItem);
                        // TODO some logging 
                    }
                }
                else
                    Console.WriteLine("Could not get data");
            }

            outputData?.CompleteAdding();
        }
        
        public void Run(string dirPath = "../../Data/paintings")
        {
            var images = Directory.GetFiles(dirPath);
            
            inputData = new BlockingCollection<string>(10);
            outputData = new BlockingCollection<Image<Rgba32>>(10);
          
            function = async filename =>
            {
                byte[] result;
                using (FileStream sourceStream = File.Open(filename, FileMode.Open))
                {
                    result = new byte[sourceStream.Length];
                    await sourceStream.ReadAsync(result, 0, (int)sourceStream.Length);
                }

                return Image.Load(result);
            };
            
            // TODO             
            // Stage 2 
            // Resize image 
            // ImageHandler.Resize(image, width, height)

            // Stage 3
            // Create 3D image
            // ImageHandler.ConvertTo3D image
            
            // Stage 4
            // Save image to FileSystem
            
            // Combine stages and run
        }
    }
}
