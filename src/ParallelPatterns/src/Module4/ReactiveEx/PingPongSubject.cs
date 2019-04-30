using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Reactive
{
    // TODO implement the interface "ISubject<Pong, Ping>"
    public class Ping //: ISubject<Ping, Pong>
    {

        // Subscribes an observer to the observable sequence.
        public IDisposable Subscribe(IObserver<Ping> observer)
        {
            // TODO 
            // implement an Observable timer that sends a notification
            // to the subscriber ("Pong") every 1 second
            // Suggestion, the trick is to send to the observer a message
            // that contains this instance of PING
            return null;
        }
        // Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    }

    // TODO implement the interface "ISubject<Ping, Pong>"
    public class Pong 
    {
        // Subscribes an observer to the observable sequence.
        public IDisposable Subscribe(IObserver<Pong> observer)
        {
            // TODO
            // implement an Observable timer that sends a notification
            // to the subscriber ("Ping") every 1.5 second
            // Suggestion, the trick is to send to the observer a message
            // that contains this instance of PONG
            return null;
        }       
    }
}
