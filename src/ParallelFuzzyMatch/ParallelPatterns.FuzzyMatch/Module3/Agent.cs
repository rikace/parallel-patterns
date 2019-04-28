using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ParallelPatterns.Common;
using ParallelPatterns.Fsharp;
using static ParallelPatterns.Common.OptionHelpers;
using static ParallelPatterns.Fsharp.Interfaces;

namespace ParallelPatterns
{
    public static class Agent
    {
        public static IAgent<TMessage, TState> Start<TMessage, TState>(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action, 
            CancellationTokenSource cts = null)
            => new Agent<TMessage, TState>(initialState, action, cts);

        public static IAgent<TMessage, TState> Start<TMessage, TState>(
            TState initialState,
            Func<TState, TMessage, TState> action, 
            CancellationTokenSource cts = null)
            => new Agent<TMessage, TState>(initialState, action, cts);

        public static IReplyAgent<TMessage, TReply> Start<TState, TMessage, TReply>(
            TState initialState,
            Func<TState, TMessage, Task<TState>> projection, 
            Func<TState, TMessage, Task<(TState, TReply)>> ask,
            CancellationTokenSource cts = null)
            => new AgentReply<TState, TMessage, TReply>(initialState, projection, ask, cts);

        public static IReplyAgent<TMessage, TReply> Start<TState, TMessage, TReply>(
            TState initialState,
            Func<TState, TMessage, TState> projection, 
            Func<TState, TMessage, (TState, TReply)> ask,
            CancellationTokenSource cts = null)
            => new AgentReply<TState, TMessage, TReply>(initialState, projection, ask, cts);

        public static IDisposable LinkTo<TOutput, TState>(
            this ISourceBlock<TOutput> source,
            IAgent<TOutput, TState> agent)
            => source.AsObservable().Subscribe(agent.Post);
    }

    public class Agent<TMessage, TState> : IAgent<TMessage, TState>
    {
        public Agent(
            TState initialState,
            Func<TState, TMessage, TState> action,
            CancellationTokenSource cts = null)
        {
            // TODO (7.a)
            // Implement Agent body (and behavior)
            //      - Initialize local isolated state
            // Suggestion :
            //  Create Dataflow-block that receives and processes the messages, 
            //  and then update the local state
        }
        public Agent(
            TState initialState,
            Func<TState, TMessage, Task<TState>> action,
            CancellationTokenSource cts = null)
        {
            // TODO (7.a) 
            // same as abouve but different Agent 
            // behavior signature
        }

        public Task Send(TMessage message)
            => /* TODO  missing code */ Task.CompletedTask;
        public void Post(TMessage message)
        { /* TODO  missing code */ }

        public IObservable<TState> AsObservable()
            => null; /* TODO  missing code */
    }



    public class AgentReply<TState, TMessage, TReply> : IReplyAgent<TMessage, TReply>
    {
        private TState _state;

        private readonly ActionBlock<(TMessage,
            Option<TaskCompletionSource<TReply>>)> _actionBlock;
        
        public AgentReply(TState initialState,
            Func<TState, TMessage, TState> projection,
            Func<TState, TMessage, (TState, TReply)> ask,
            CancellationTokenSource cts = null)
        {
            _state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts?.Token ?? CancellationToken.None
            };

            // TODO (7.b)
            // Implement Agent body (and behavior)
            //      - Initialize local isolated state
            // Suggestion :
            //    - Create Dataflow-block that receives and processes the messages, 
            //      and then update the local state
            //    - There are 2 type of messages, the Send that is a fire and forget,
            //      and an Ask Message that replies back to the sender.
            //      try to differenciate the two type of messages to handle the different behavior
            //    - Use the TaskCompletionSource as part of the payload message for the Ask type
        }

        public AgentReply(TState initialState,
            Func<TState, TMessage, Task<TState>> projection,
            Func<TState, TMessage, Task<(TState, TReply)>> ask,
            CancellationTokenSource cts = null)
        {
            _state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts?.Token ?? CancellationToken.None
            };


            // TODO (7.b)
            // same as before, but with different signature in the constructor
            // for the Ask behavior
            //      Func<TState, TMessage, Task<(TState, TReply)>> ask,
        }


        public Task<TReply> Ask(TMessage message)
        {
            var tcs = new TaskCompletionSource<TReply>();
            _actionBlock.Post((message, Some(tcs)));
            return tcs.Task;
        }

        public Task Send(TMessage message) =>
            _actionBlock.SendAsync((message, None));

        public void Post(TMessage message) =>
            _actionBlock.Post((message, None));
    }
}