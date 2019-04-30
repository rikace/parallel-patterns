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
    public class StatelessDataflowAgent_TODO<TMessage> : IAgent<TMessage>
    {
        public StatelessDataflowAgent_TODO(Action<TMessage> action, CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines a synchronous operation
        }

        public StatelessDataflowAgent_TODO(Func<TMessage, Task> action, CancellationTokenSource cts = null)
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
    public class StatefulDataflowAgent_TODO<TState, TMessage> : IAgent<TMessage>
    {
        private TState state;

        public StatefulDataflowAgent_TODO(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            // (1) Implement Agent with TPL DATA FLOW
            // this constructor defines an asynchronous operation to apply at the current state (combined to the message ?)
        }

        public StatefulDataflowAgent_TODO(TState initialState,
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

    #region Solution
    
    public class StatefulDataflowAgent<TState, TMessage> : IAgent<TMessage>
    {
        private TState state;
        private readonly ActionBlock<TMessage> actionBlock;

        public StatefulDataflowAgent(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ?
                    cts.Token : CancellationToken.None
            };
            actionBlock = new ActionBlock<TMessage>(
                async msg => state = await action(state, msg), options);
        }

        public Task Send(TMessage message) => actionBlock.SendAsync(message);
        public void Post(TMessage message) => actionBlock.Post(message);


        public StatefulDataflowAgent(TState initialState, Func<TState, TMessage, TState> action, CancellationTokenSource cts = null)
        {
            state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ? cts.Token : CancellationToken.None
            };
            actionBlock = new ActionBlock<TMessage>(
                msg => state = action(state, msg), options);
        }

        public TState State => state;
    }


    public class StatelessDataflowAgent<TMessage> : IAgent<TMessage>
    {
        private readonly ActionBlock<TMessage> actionBlock;

        public StatelessDataflowAgent(Action<TMessage> action, CancellationTokenSource cts = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ? cts.Token : CancellationToken.None
            };
            actionBlock = new ActionBlock<TMessage>(action, options);
        }

        public StatelessDataflowAgent(Func<TMessage, Task> action, CancellationTokenSource cts = null)
        {
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts == null ? cts.Token : CancellationToken.None
            };
            actionBlock = new ActionBlock<TMessage>(action, options);
        }

        public void Post(TMessage message) => actionBlock.Post(message);
        public Task Send(TMessage message) => actionBlock.SendAsync(message);

    }
  
    

    public class StatefulDataflowAgentWithRx<TState, TMessage> : IAgentRx<TMessage, TState>
    {
        private TState state;
        private TransformBlock<TMessage, TState> block;

        public StatefulDataflowAgentWithRx(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ?
                    cts.Token : CancellationToken.None
            };
            block = new TransformBlock<TMessage, TState>(
                async msg => state = await action(state, msg), options);
        }

        public Task Send(TMessage message) => block.SendAsync(message);
        public void Post(TMessage message) => block.Post(message);
        public IObservable<TState> Observable() => block.AsObservable();
        public TState State => state;
    }

    
    
    #endregion

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

            var agentStateful_TODO = Agent.Start<Dictionary<string, string>>(msg => { });
            // (2) complete this code
            urls.ForEach(url =>
            {
                /* agentStateful.Post(url) */
            });

            #region Solution

            var agentStateful = Agent.Start(ImmutableDictionary<string, string>.Empty,
                async (ImmutableDictionary<string, string> state, string url) =>
                {
                    if (!state.TryGetValue(url, out string content))
                        using (var webClient = new WebClient())
                        {
                            content = await webClient.DownloadStringTaskAsync(url);
                            await File.WriteAllTextAsync(createFileNameFromUrl(url), content);
                            return state.Add(url, content);
                        }

                    return state;
                });

            #endregion
        }
    }
}