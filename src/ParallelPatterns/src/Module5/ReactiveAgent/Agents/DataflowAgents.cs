using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ReactiveAgent.Agents.Dataflow
{
    //  Agents in C# using TPL Dataflow

    // TODO : 5.2
    // (1) implement an Agent using the TPL Dataflow
    // the Agent is stateless
    public class StatelessDataflowAgent<TMessage> : IAgent<TMessage>
    {
        public StatelessDataflowAgent(Action<TMessage> action, CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines a synchronous operation
        }

        public StatelessDataflowAgent(Func<TMessage, Task> action, CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines an asynchronous operation
        }

        public void Post(TMessage message)
        {
        } // (3) complete this code to post a message to the agent

        public Task Send(TMessage message) =>
            null; // (2) complete this code to send a message to the agent Asynchronously

    }

    // TODO : 5.2
    // (1) implement an Agent using the TPL Dataflow
    // the Agent should be capable to maintains an internal state
    public class StatefulDataflowAgent<TState, TMessage> : IAgent<TMessage>
    {
        private TState state;

        public StatefulDataflowAgent(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines an asynchronous operation to apply at the current state (combined to the message ?)
        }

        public StatefulDataflowAgent(TState initialState,
            Func<TState, TMessage, TState> action,
            CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines a synchronous operation to apply at the current state (combined to the message ?)
        }

        public Task Send(TMessage message) =>
            null; // (2) complete this code to send a message to the agent Asynchronously

        public void Post(TMessage message)
        {
        } // (3) complete this code to post a message to the agent

        public TState State => state;
    }
    
    public class StatefulDataflowAgentWithRx<TState, TMessage> : IAgentRx<TMessage, TState>
    {
       
        public StatefulDataflowAgentWithRx(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            // (1) Implement Agent RX with TPL DATA FLOW
            // this constructor defines an asynchronous operation to apply at the current state (combined to the message ?)
        }

        public Task Send(TMessage message) => 
            // complete this code to send a message to the agent Asynchronously
            null;

        public void Post(TMessage message)
        {
            // complete this code to send a message to the agent 
        }

        public IObservable<TState> Observable() => 
            // complete this code to support RX that triggers every time a message is processed
            null;
        public TState State => 
            // complete this code to expose the local State
            default(TState);
    }

    public class StatefulDataflowAgentSample
    {
        public string createFileNameFromUrl(string url) => Path.Combine(Path.GetTempPath(), Path.GetTempFileName());

        public void Run()
        {
            //   Producer/consumer using TPL Dataflow
            List<string> urls = new List<string>
            {
                @"http://www.google.com",
                @"http://www.microsoft.com",
                @"http://www.bing.com",
                @"http://www.google.com"
            };


            // TODO 5.3
            // Agent fold over state and messages - Aggregate
            urls.Aggregate(ImmutableDictionary<string, string>.Empty,
                (state, url) =>
                {
                    if (!state.TryGetValue(url, out string content))
                        using (var webClient = new WebClient())
                        {
                            content = webClient.DownloadString(url);
                            System.IO.File.WriteAllText(createFileNameFromUrl(url), content);
                            return state.Add(url, content);
                        }

                    return state;
                });

            // TODO : 5.3
            // (1) replace the implementation using the urls.Aggregate with a new one that uses an Agent
            // Suggestion, instead of the Dictionary you should try to use an immutable structure

            var agentStateful = Agent.Start<Dictionary<string, string>>(msg => { });
            
            // (2) complete this code
            // urls.ForEach(url => { agentStateful.Post(url); });

          
        }
    }
}