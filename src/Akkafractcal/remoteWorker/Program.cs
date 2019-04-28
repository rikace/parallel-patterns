using System;
using Akka.Actor;
using Akka.Configuration;

namespace remoteWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = ConfigurationFactory.ParseString(@"
akka {  
    log-config-on-start = on
    stdout-loglevel = DEBUG
    loglevel = DEBUG
    actor {
        provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
        
        debug {  
          receive = on 
          autoreceive = on
          lifecycle = on
          event-stream = on
          unhandled = on
        }
    }
    remote {
        dot-netty.tcp {
		    port = 8080
		    hostname = 127.0.0.1
        }
    }
}
");
            Console.Title = "Remote Worker";

            using (var system = ActorSystem.Create("RemoteSystem", config))
            {
                Console.ReadLine();
            }
        }
    }
}
