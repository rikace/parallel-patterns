using ParallelPatterns;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Console;
using ReactiveAgent.Agents;

namespace TPLAgent
{
   public class PingPongAgents
    {
        public static void Start()
        {
            IAgent<string> logger, ping = null;
            IAgent<string> pong = null;

            logger = Agent.Start((string msg) => WriteLine(msg));

            ping = Agent.Start((string msg) =>
            {
                if (msg == "STOP") return;

                logger.Post($"Received '{msg}'; Sending 'PING'");
                Task.Delay(500).Wait();
                pong.Post("PING");
            });

            pong = Agent.Start(0, (int count, string msg) =>
            {
                int newCount = count + 1;
                string nextMsg = (newCount < 5) ? "PONG" : "STOP";

                logger.Post($"Received '{msg}' #{newCount}; Sending '{nextMsg}'");
                Task.Delay(500).Wait();
                ping.Post(nextMsg);

                return newCount;
            });

            ping.Post("START");

        }
    }
}