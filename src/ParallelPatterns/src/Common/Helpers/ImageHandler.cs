using System;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;
using SixLabors.ImageSharp.Processing.Processors.Overlays;
using SixLabors.Primitives;

namespace Helpers
{
    public static  class ImageHandler
    {
        private static readonly Configuration configuration = new Configuration(new JpegConfigurationModule());
        public static Image<Rgba32> Resize(Image<Rgba32> source, int newWidth, int newHeight)
        {
            // this.configuration.MaxDegreeOfParallelism = Environment.ProcessorCount;
            var image = source.Clone();
            image.Mutate(x => x.Resize(400, 400));
            return image;
        }

        public static Image<Rgba32> ConvertTo3D(Image<Rgba32> source)
        {
            var image = source.Clone();
            var w = image.Width;
            var h = image.Height;
            for (int x = 20; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var c1 = image[x, y];
                    var c2 = image[x - 20, y];
                    image[x - 20, y] = new Rgba32(c1.R, c2.G, c2.B);
                }
            }
            return image;
        }


        public static Image<Rgba32> SetFilter(Image<Rgba32> source, ImageFilters imageFilters)
        {
            var image = source.Clone();
            var w = image.Width;
            var h = image.Height;
            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var p = image[x, y];
                    if (imageFilters == ImageFilters.Red)
                    {
                        p.B = 0;
                        p.G = 0;
                    }
                    else if(imageFilters == ImageFilters.Green)
                    {
                        p.R = 0;
                        p.B = 0;
                    }
                    else if (imageFilters == ImageFilters.Blue)
                    {
                        p.R = 0;
                        p.G = 0;
                    }
                    else if (imageFilters == ImageFilters.Gray)
                    {
                        var gray = (byte)((0.299 * (double)p.R) + (0.587 * (double)p.G) + (0.114 * (double)p.B));
                        p.R = gray;
                        p.G = gray;
                        p.B = gray;
                    }
                    image[x, y] = p; 
                }
            }
            return image;
        }
    }

    public enum ImageFilters
    {
        Red,Green,Blue,Gray
    }
}
        