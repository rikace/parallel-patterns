using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ActorMapReduceWordCount.Messages;
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
     
        public LineReaderActor(IWriteStuff writer)
        {
            _writer = writer;
     		 SetupBehaviors();
	    }

        private void SetupBehaviors()
        {
            Receive<ReadLineForCounting>(msg =>
            {
                var cleanFileContents = Regex.Replace(msg.Line, @"[^\u0000-\u007F]", " ");
                var wordCounts = new Dictionary<String, Int32>();
                
                var wordArray = cleanFileContents.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in wordArray)
                {
                    var wordCounter = Context.Child(word);
                    if (wordCounter.IsNobody())
                    {
                        if (wordCounts.ContainsKey(word))
                        {
                            wordCounts[word] += 1;
                        }
                        else
                        {
                            wordCounts.Add(word, 1);
                        }
                    }
                    Sender.Tell(new MappedList(msg.LineNumber, wordCounts));
                    
                }
            });

            Receive<Complete>(msg =>
            {   
                Sender.Tell(msg);
            });
        }
    }
}
