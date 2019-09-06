using System;
using System.IO;
using Microsoft.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AkkaFractal.Core
{
    public static class BitmapConverter
    {
        private static RecyclableMemoryStreamManager streamManager = new RecyclableMemoryStreamManager();
        
        // TODO Stream Reuse resource 
        public static byte[] ToByteArray(this Image<Rgba32> imageIn)
        {
            using (var ms = streamManager.GetStream("BitmapConverter"))
            {
                var options = new PngEncoder
                {
                    Quantizer = KnownQuantizers.WebSafe
                };
                imageIn.SaveAsPng(ms, options);
                return ms.ToArray();
            }
        }

        public static string ToBase64Png(this Image<Rgba32> imageIn)
        {
            var bytes = imageIn.ToByteArray();
            return Convert.ToBase64String(bytes);
        }
        
        
        public static Image<Rgba32> ToBitmap(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = streamManager.GetStream("BitmapConverter", byteArrayIn, 0, byteArrayIn.Length))
            {
                var returnImage = Image.Load(byteArrayIn);
                return returnImage;
            }
        }

        public static Image<Rgba32> ToBitmap(this string base64Png)
        {
            return Image.Load(Convert.FromBase64String(base64Png));
        }
    }
}