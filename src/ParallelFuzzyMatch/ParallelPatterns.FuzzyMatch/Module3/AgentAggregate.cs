using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net.Http;

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



            urls.Aggregate(ImmutableDictionary<string, string>.Empty,
                (state, url) =>
                {
                    if (state.TryGetValue(url, out var content)) 
                        return state;
                    
                    using (var webClient = new HttpClient())
                    {
                        System.Console.WriteLine($"Downloading '{url}' sync ...");
                        content = webClient.GetStringAsync(url).GetAwaiter().GetResult();
                        File.WriteAllText(CreateFileNameFromUrl(url), content);
                        return state.Add(url, content);
                    }
                });

            // TODO (8)
            // Agent fold over state and messages example 
            // Replace the implementation urls.Aggregate with a new one 
            // that uses an Agent construct             
            var agentStateful = Agent.Start(new Dictionary<string, string>(),
                (Dictionary<string, string> state, string msg) => state);
            

            // run this code 
            urls.ForEach(agentStateful.Post);
        }
    }
}