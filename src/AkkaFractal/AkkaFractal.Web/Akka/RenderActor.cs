using System;
using System.Threading.Tasks;
using Akka.Actor;
using Lib.AspNetCore.ServerSentEvents;
using AkkaFractal.Core;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AkkaFractal.Web.Akka
{
    public class RenderActor : ReceiveActor
    {
        string destination = @"./image.jpg";
        private Image<Rgba32> image;
        public RenderActor(IServerSentEventsService serverSentEventsService, int width, int height, int split) 
        {
            image = new Image<Rgba32>(width, height);
            var ys = height / split;
            var xs = width / split;

            var totalMessages = ys + xs;
            Console.WriteLine($"YS {ys}");
            Console.WriteLine($"totalMessages {totalMessages}");
  
            int count = 0;

            Func<RenderedTile, Task> renderedTileAction = async tile =>
            {
                var sseTile = new SseFormatTile(tile.X, tile.Y, Convert.ToBase64String(tile.Bytes));
                var text = JsonConvert.SerializeObject(sseTile);
                await serverSentEventsService.SendEventAsync(text);

                Console.WriteLine($"Received Message {++count}");

                totalMessages--;
                var tileImage = tile.Bytes.ToBitmap();
                var xt = 0;
                for (int x = 0; x < xs; x++)
                {
                    int yt = 0;
                    for (int y = 0; y < ys; y++)
                    {
                        image[x + tile.X, y + tile.Y] = tileImage[x, y];
                        yt++;
                    }

                    xt++;
                }
            };

            Action<Completed> complete = _ =>
            {
                image.Save(destination);
                Console.WriteLine("Tile render completed");
            };

            // TODO 
            // implement the two Actor Receiver that handle the message types:
            // - RenderedTile
            // - Completed
         
            // CODE HERE
        }
    }
}