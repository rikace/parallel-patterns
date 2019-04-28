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
    public class WordCountAggregatorActor : ReceiveActor
    {

        private IWriteStuff _writer;
       
        public static Props Create(IWriteStuff writer)
        {
            return Props.Create(() => new WordCountAggregatorActor(writer));
        }



        public WordCountAggregatorActor(IWriteStuff writer)
        {
            _writer = writer;

            // TODO
            // implement logic to keep track of the words count (may be with a dictionary)
            // using a local Receive function that receives a message from the
            // "WordCounterActor" actor for the word-count.
            // 
            // Then implement a Receive function that receives a message to display the total-word-count (may be top 25)
            // The sender of this message is the "LineReaderActor" actor when completes the count
            
            //  Receive<WordCount>(msg =>
        }

        private void DetermineIfCountsShouldBeDisplayed()
        {
            // this method will run when the message "Complete" arrives 
            // all words are there...display the top 25 in order
        }
    }
}
