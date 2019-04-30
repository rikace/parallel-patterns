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

            

            // TODO 
            // register the Ping and Pong (Observable/Observer) to each other
            // var pongSubscription
            // var pingSubscription
            
            Console.WriteLine("Press any key to stop ...");

            Console.ReadKey();

            //pongSubscription.Dispose();
            //pingSubscription.Dispose();

            Console.WriteLine("Ping Pong has completed.");
        }
    }
}
