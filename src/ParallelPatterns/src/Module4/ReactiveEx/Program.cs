using Reactive;
using System;

namespace ReactiveEx
{
    class Program
    {
        static void Main(string[] args)
        {
            var ping = new Ping();
            var pong = new Pong();

            // TODO : uncomment the following code
            // and implement the ISubject interface for both 
            // the Ping & Pong classes
            //
            // register the Ping and Pong (Observable/Observer) to each other
            // var pongSubscription = ping.Subscribe(pong);
            // var pingSubscription = pong.Subscribe(ping);
            // 
            // Console.WriteLine("Press any key to stop ...");
            // Console.ReadKey();
            //
            // pongSubscription.Dispose();
            // pingSubscription.Dispose();

            Console.WriteLine("Ping Pong has completed.");


            Console.ReadLine();
            
            // TODO Rx example
            // AsyncToObservable.Start();
        }
    }
}
