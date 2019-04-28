using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Akka.Actor;
using ActorWordCounter.Messages;
using ActorWordCounter.Writers;

namespace ActorWordCounter.Actors
{
    public class LineReaderActor  : ReceiveActor
    {
        public static Props Create(IWriteStuff writer)
        {
            return Props.Create(() => new LineReaderActor(writer));
        }

        private readonly IWriteStuff _writer;
        private IActorRef _wordAggregator;

        public LineReaderActor(IWriteStuff writer)
        {
            _writer = writer;
            _wordAggregator = Context.ActorOf(WordCountAggregatorActor.Create(writer), "aggregator");

            Receive<ReadLineForCounting>(msg =>
            {
                var cleanFileContents = Regex.Replace(msg.Line, @"[^\u0000-\u007F]", " ");

                var wordArray = cleanFileContents.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in wordArray)
                {
                    var wordCounter = Context.Child(word);
                    if (wordCounter.IsNobody())
                    {
                        // Child Actor created for each word. 
                        // The instance of this actor uses the WordCountAggregatorActor 
                        wordCounter = Context.ActorOf(WordCounterActor.Create(_writer, _wordAggregator, word), word);
                    }

                    wordCounter.Tell(new CountWord());
                }
            });

            Receive<Complete>(msg =>
            {
                var childCount = -1;     
                
                // TODO 
                // get the reference to all the "Children" Actors in this context
                // and dispatch to all the message "DisplayWordCount".
                // Keep track of the total "childCount"
                
               
                _wordAggregator.Tell(new TotalWordCount(childCount));
            });
        }
    }
}
