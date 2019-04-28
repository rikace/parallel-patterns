using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ActorWordCounter.Messages;
using ActorWordCounter.Writers;

namespace ActorWordCounter.Actors
{
    public class CountSupervisor : ReceiveActor
    {
        public static Props Create(IWriteStuff writer)
        {
            return Props.Create(() => new CountSupervisor(writer));
        }

        private readonly IWriteStuff _writer;
        public CountSupervisor(IWriteStuff writer)
        {
            _writer = writer;
            Receive<StartCount>(msg =>
            {
                var fileInfo = new FileInfo(msg.FileName);

                // TODO 
                // Create a child reader actor (LineReaderActor) using the current Context
                // Then use the FileInfo "fileInfo" to read the content 
                // and push each line of text to the reader actor created.
                // use the message types "ReadLineForCounting" and "Completed"

             
            });
        }
    }
}
