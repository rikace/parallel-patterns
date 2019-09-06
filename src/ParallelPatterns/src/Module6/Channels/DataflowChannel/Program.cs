using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataflowChannel
{
    class ImageInfo
    {
        public string Path { get; set; }
        public string Name { get; set; }
        public Image<Rgba32> Image { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var chanLoadImage = new ChannelAgent<string>();
            var chanApply3DEffect = new ChannelAgent<ImageInfo>();
            var chanSaveImage = new ChannelAgent<ImageInfo>();

            // TODO : 6.7
            // Update the "Subscribe" function in the way that accepts an optional collections of Channels as last arguments.
            // If this argument is present, the output of the channel is broadcast to all the channels
            chanLoadImage.Subscribe(image =>
            {
                var imageInfo = new ImageInfo
                {
                    Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                    Name = Path.GetFileName(image),
                    Image = Image.Load(image)
                };
                chanApply3DEffect.Send(imageInfo);
            });

            chanApply3DEffect.Subscribe(imageInfo =>
            {
                imageInfo.Image = ConvertImageTo3D(imageInfo.Image);
                chanSaveImage.Send(imageInfo);
            });

            // ImageProcessing.ImageHandler.SetGrayscale()
            // ImageProcessing.ImageHandler.SetColorFilter()\
            chanSaveImage.Subscribe(imageInfo =>
            {
                Console.WriteLine($"Saving image {imageInfo.Name}");
                var destination = Path.Combine(imageInfo.Path, imageInfo.Name);
                imageInfo.Image.Save(destination);
            });

            var images = Directory.GetFiles("../../../../../Data/paintings");

            foreach (var image in images)
                chanLoadImage.Send(image);

            Console.ReadLine();
            TaskPool.Stop();
        }

        private static Image<Rgba32> ConvertImageTo3D(Image<Rgba32> image)
        {
            return Helpers.ImageHandler.ConvertTo3D(image);
        }
    }
}
