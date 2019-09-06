using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ActorMapReduceWordCount.Messages;
using Akka.Actor;
using ActorWordCounter.Messages;
using ActorWordCounter.Writers;
using Akka.Routing;

namespace ActorWordCounter.Actors
{
    public class CountSupervisor : ReceiveActor
    {
        public static Props Create(IWriteStuff writer, Stopwatch sw)
        {
            return Props.Create(() => new CountSupervisor(writer, sw));
        }

        private readonly IWriteStuff _writer;
        private Dictionary<String, Int32> _wordCount;
        private readonly Int32 _numberOfRoutees;
        private Int32 _completeRoutees;
        private Stopwatch _stopwatch;

        public CountSupervisor(IWriteStuff writer, Stopwatch sw)
        {
            _writer = writer;
            _wordCount = new Dictionary<String, Int32>();
            _numberOfRoutees = 5;
            _completeRoutees = 0;
            _stopwatch = sw;

            SetupBehaviors(writer);
        }

        private void SetupBehaviors(IWriteStuff writer)
        {
            Receive<StartCount>(msg =>
            {
                var lineNumber = 0;
                
                var fileInfo = new FileInfo(msg.FileName);
				// TODO 
                // (1) Create a child reader actor (LineReaderActor) using the current Context.
                    // To instantiate an actor you should use a "Props".
                    // for true parallelism, instantiate the actor using a Pool that load-balance
                    // the work across the actor-children.
                
                // (2) Use the previous FileInfo "fileInfo" to read the text content,  
                // and then push each line of the text to the reader actor created in (1).
                // use the message types "ReadLineForCounting" and then complete when all the
                // lines have been sent "Completed".
                
                // Note: the message "ReadLineForCounting" take a line-number, so the 
                //       variable "var lineNumber = 0;" could be helpful here.
				
                // Note: if you have instantiated the LineReaderActor actor using a pool, the 
                //       "Complete" message should be sent to all the children (Broadcast)

                // CODE HERE
            });

            Receive<MappedList>(msg =>
            {
                foreach (var key in msg.LineWordCount.Keys)
                {
                    if (_wordCount.ContainsKey(key))
                    {
                        _wordCount[key] += msg.LineWordCount[key];
                    }
                    else
                    {
                        _wordCount.Add(key, msg.LineWordCount[key]);
                    }
                }
            });

            Receive<Complete>(msg =>
            {
                _completeRoutees++;

                if (_completeRoutees == _numberOfRoutees)
                {
                    var topWords = _wordCount.OrderByDescending(w => w.Value).Take(25);
                    foreach (var word in topWords)
                    {
                        _writer.WriteLine($"{word.Key} == {word.Value} times");
                    }

                    _stopwatch.Stop();
                    writer.WriteLine($"Elapsed time: {_stopwatch.ElapsedMilliseconds}");
                }
            });
        }
    }
}