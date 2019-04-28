using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ActorWordCounter.Messages;
using ActorWordCounter.Writers;

namespace ActorWordCounter.Actors
{
    public class WordCounterActor : ReceiveActor
    {

        public static Props Create(IWriteStuff writer, IActorRef aggregator, String word)
        {
            return Props.Create(() => new WordCounterActor(writer, aggregator, word));
        }

        private readonly IWriteStuff _writer;
        private IActorRef _aggregator;
        private String _theWord;
        private Int32 _count;
        public WordCounterActor(IWriteStuff writer, IActorRef aggregator, String word)
        {
            _writer = writer;
            _theWord = word;
            _count = 0;
            _aggregator = aggregator;

            Receive<CountWord>(msg =>
            {
                _count++;
            });

            Receive<DisplayWordCount>(msg =>
            {  
                _aggregator.Tell(new WordCount(_theWord, _count));
            });
        }
    }
}
