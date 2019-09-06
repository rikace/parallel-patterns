using ReactiveAgent.Agents;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ReactiveAgent.Agents.Dataflow;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using ParallelPatterns;
using TPLAgent;

namespace ReactiveAgent.CS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            
            // TODO 
            // complete Dataflow and the run this code
            
            DataflowPipeline.DataflowPipeline.Start();
            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();

            // DEMO
            // PingPongAgents.Start();
            //Console.ReadLine();

            //   WordCountAgentsExample.Run().Wait();
            //   Console.WriteLine("Finished. Press any key to exit.");
            //   Console.ReadLine();
		
            
             // DEMO
             //   DataflowTransformActionBlocks.Run();
             //   Console.WriteLine("Finished. Press any key to exit.");
             //   Console.ReadLine();

            // TODO 
            //   AgentAggregate.Run();
            //   Console.WriteLine("Finished. Press any key to exit.");
            //   Console.ReadLine();
        }
    }
}