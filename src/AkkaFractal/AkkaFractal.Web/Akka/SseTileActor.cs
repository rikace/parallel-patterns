using System;
using Akka.Actor;
using Lib.AspNetCore.ServerSentEvents;
using AkkaFractal.Core;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace AkkaFractal.Web.Akka
{
    public delegate IActorRef SseTileActorProvider();

    public class SseTileActor : ReceiveActor
    {
        private IActorRef renderActor;
        public SseTileActor(IServerSentEventsService serverSentEventsService, IActorRef tileRenderActor)
        {
            Receive<RenderImage>(request =>
            {
                var split = 20;
                var ys = request.Height / split;
                var xs = request.Width / split;

                // TODO
                // Complete the "RenderActor" actor in the "Akka" folder.
                // This Actor should receive two message types:
                // - "RenderedTile" that uses the local lambda "renderedTileAction"
                // - "Complete" that triggers the local lambda "complete"
                
                // TODO replace this line of code with the code
                // that instantiate the renderActor IActorRef as a Child
                // NOTE: This actor should be implemented only once if and only if 
                //       it is not instantiated yet (use the local "Context" to check) 
                // NOTE: To create a Child Actor you should use the current "Context"
                
                renderActor = Nobody.Instance;
            
                for (var y = 0; y < split; y++)
                {
                    var yy = ys * y;
                    for (var x = 0; x < split; x++)
                    {
                        var xx = xs * x;
                        
                        // TODO
                        // pass the previously instantiated "renderActor" IActorRef as the "Sender" of the following "tileRenderActor" Message-Payload.
                        // in this way, when the "tileRenderActor" completes the computation, the response send with "Sender.Tell" will be sent
                        // to the "renderActor" actor rather then the current "SseTileActor"
                        tileRenderActor.Tell(new RenderTile(yy, xx, xs, ys, request.Height, request.Width));
                    }
                }

                // TODO
                // Same as previous TODO 
                // 
                // pass the previously instantiated "renderActor" IActorRef as the "Sender" of the following "tileRenderActor" Message-Payload.
                // in this way, when the "tileRenderActor" completes the computation, the response send with "Sender.Tell" will be sent
                // to the "renderActor" actor rather then the current "SseTileActor"
                tileRenderActor.Tell(new Completed());
            });
        }
    }
}