using System;
using ReactiveAgent.Agents;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IO = System.IO;
using File = ReactiveAgent.Agents.File;

namespace ParallelPatterns
{
    public class AgentAggregate
    {
        private static string CreateFileNameFromUrl(string _) =>
            Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

        public static void Run()
        {
            // Producer/consumer using TPL Dataflow
            var urls = new List<string>
            {
                @"http://www.google.com",
                @"http://www.microsoft.com",
                @"http://www.bing.com",
                @"http://www.google.com"
            };

            // TODO (8
            // Agent fold over state and messages - Aggregate
            urls.Aggregate(ImmutableDictionary<string, string>.Empty,
                (state, url) =>
                {
                    if (state.TryGetValue(url, out var content))
                        return state;

                    using (var webClient = new HttpClient())
                    {
                        System.Console.WriteLine($"Downloading '{url}' sync ...");
                        content = webClient.GetStringAsync(url).GetAwaiter().GetResult();
                        IO.File.WriteAllText(CreateFileNameFromUrl(url), content);
                        return state.Add(url, content);
                    }
                });

            // TODO (8)  
            // replace the implementation using the urls.Aggregate with a new one that uses an Agent.
            var agentStateful = 
                Agent.StartWithRx(ImmutableDictionary<string, string>.Empty,
                async (ImmutableDictionary<string, string> state, string msg) =>
                {
                    if (state.TryGetValue(msg, out var content))
                        return state;

                    using (var webClient = new HttpClient())
                    {
                        System.Console.WriteLine($"Downloading '{msg}' async ...");
                        content = await webClient.GetStringAsync(msg);
                        //IO.File.WriteAllText(CreateFileNameFromUrl(msg), content);
                        state.Add(msg, content);
                        return state;
                    }
                });

            agentStateful.Observable().Subscribe(state =>
            {
                var lastUrl = state.Last().Key;
                var lastHtmlPage = state.Last().Value;

                Console.WriteLine($"Downloaded {lastUrl} with len {lastHtmlPage.Length}");
            });
            
            // run this code 
            urls.ForEach(agentStateful.Post);
        }
    }
}