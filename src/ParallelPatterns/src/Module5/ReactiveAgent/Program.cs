using ReactiveAgent.Agents;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using ReactiveAgent.Agents.Dataflow;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;
using ParallelPatterns;

namespace ReactiveAgent.CS
{
    public class Program
    {
      
        public static void Main(string[] args)
        {
            //PingPongAgents.Start();
            //Console.ReadLine();
  
            WordCountAgentsExample.Run().Wait();

            Console.ReadLine();

            DataflowPipeline.DataflowPipeline.Start();

            Console.WriteLine("Finished. Press any key to exit.");
            Console.ReadLine();

        }

        static async Task Play()
        {
            (new DataflowTransformActionBlocks()).Run();

            AgentAggregate.Run();

            await WordCountAgentsExample.Run();
        }
    }
}
