using System;

using Akka.Actor;

using AkkaFractal.Core;

namespace AkkaFractal.Remote
{
    class Program
    {
        static void Main(string[] args)
        {    
            var config = ConfigurationLoader.Load();
            using (var system = ActorSystem.Create("RemoteSystem", config))
            {
                Console.Title = $"Remote Worker - {system.Name}";
                Console.ForegroundColor = ConsoleColor.Green;
                
                Console.WriteLine("Press [ENTER] to exit.");
                Console.ReadLine();
            }
        }
    }
}
