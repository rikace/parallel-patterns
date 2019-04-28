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

            Console.WriteLine("Press any key to stop ...");

            // TODO 
            // register the Ping and Pong (Observable/Observer) to each other
            // var pongSubscription
            // var pingSubscription

            Console.ReadKey();

            //pongSubscription.Dispose();
            //pingSubscription.Dispose();

            Console.WriteLine("Ping Pong has completed.");
        }
    }
}
