using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using ParallelPatterns.TaskComposition;
using Helpers;

namespace ConsoleTaskEx
{
    public class ImageProcessing
    {
        struct ImageInfo
        {
            public string Name { get; set; }
            public string Source { get; set; }
            public string Destination { get; set; }
            public Image<Rgba32> Image { get; set; }
        }

        private readonly string source;
        private readonly string destination;

        public ImageProcessing(string source, string destination)
        {
            this.source = source;
            this.destination = destination;

            loadImage_Step1 = async path =>
            {
                Console.WriteLine($"Loading Image {Path.GetFileName(path)}...");
                var image = Image.Load(path);
                return new ImageInfo
                {
                    Name = Path.GetFileName(path),
                    Source = path,
                    Destination = this.destination,
                    Image = image
                };
            };

            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);
        }

        Func<string, Task<ImageInfo>> loadImage_Step1;

        Func<ImageInfo, Task<ImageInfo>> scaleImage_Step2 = async imageInfo =>
        {
            Console.WriteLine($"Scaling Image {Path.GetFileName(imageInfo.Name)}...");
            var scale = 200;
            var image = imageInfo.Image;
            var resizedImage = ImageHandler.Resize(image, scale, scale);
            imageInfo.Image = resizedImage;
            return imageInfo;
        };

        Func<ImageInfo, Task<ImageInfo>> convertTo3D_Step3 = async imageInfo =>
        {
            Console.WriteLine($"Converting to 3D Image {Path.GetFileName(imageInfo.Name)}...");
            var image = imageInfo.Image;
            var converted3DImage = ImageHandler.ConvertTo3D(image);
            imageInfo.Image = converted3DImage;
            return imageInfo;
        };

        Func<ImageInfo, Task<string>> saveImage_Step4 = async imageInfo =>
        {
            var filePathDestination = Path.Combine(imageInfo.Destination, imageInfo.Name);
            Console.WriteLine($"Saving Image {filePathDestination}...");

            imageInfo.Image.Save(filePathDestination);
            imageInfo.Image.Dispose();
            return filePathDestination;
        };

        public async Task RunContinuation()
        {
            var files = Directory.GetFiles(source, "*.jpg");

            // Task Continuation 
            foreach (string fileName in files)
            {
                await loadImage_Step1(fileName)
                    .ContinueWith(imageInfo =>
                    {
                        return scaleImage_Step2(imageInfo.Result);
                    }).Unwrap()
                    .ContinueWith(imageInfo =>
                    {
                        return convertTo3D_Step3(imageInfo.Result);
                    }).Unwrap()
                    .ContinueWith(imageInfo =>
                    {
                        saveImage_Step4(imageInfo.Result);
                    });
            }
        }



        public async Task RunTransformer()
        {
            // namespace 
            // ParallelPatterns.TaskComposition

            // Bonus: use the cancellation token to stop the computation 
            var cts = new CancellationTokenSource();

            var files = Directory.GetFiles(source, "*.jpg");

            // TODO
            // Implement the missing code for the 
            // Task Then
            // Task Select
            // Task SelectMany 
            // in "Common/Helpers.TaskComposition.cs"

            Func<string, Task<ImageInfo>> transformer = imagePath =>
                from image in loadImage_Step1(imagePath)
                from scaleImage in scaleImage_Step2(image)
                from converted3DImage in convertTo3D_Step3(scaleImage)
                select converted3DImage;

            foreach (string fileName in files)
                await transformer(fileName).Then(saveImage_Step4);
            // Option using Task.WhenAll

            await Task.WhenAll(
                files.Select(fileName => transformer(fileName).Then(saveImage_Step4))
            );
        }

        public void RunPipeline()
        {
            // Bonus: use the cancellation token to stop the computation
            var cts = new CancellationTokenSource();

            var files = Directory.GetFiles(source, "*.jpg");

            // TODO 
            // Complete the Pipeline missing code
            // "Pipeline/Pipeline.cs"
            var imagePipe =
                ParallelPatterns.Pipeline<string, ImageInfo>.Create(loadImage_Step1);

            imagePipe.Then(scaleImage_Step2)
                .Then(convertTo3D_Step3)
                .Then(saveImage_Step4);

            foreach (string fileName in files)
                imagePipe.Enqueue(fileName);

        }
    }
}