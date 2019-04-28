using ParallelPatterns.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static ParallelPatterns.Common.OptionHelpers;

namespace ReactiveAgent.Agents.Dataflow
{
    //   Producer/consumer using TPL Dataflow
    public class StatefulReplyDataflowAgent<TState, TMessage, TReply> :
        IReplyAgent<TMessage, TReply>
    {
        private TState state;

        private readonly ActionBlock<(TMessage,
            Option<TaskCompletionSource<TReply>>)> actionBlock;

        public Task<TReply> Ask(TMessage message)
        {
            var tcs = new TaskCompletionSource<TReply>();
            actionBlock.Post((message, Some(tcs)));
            return tcs.Task;
        }

        public Task Send(TMessage message) =>
            actionBlock.SendAsync((message, None));

        public void Post(TMessage message) =>
            actionBlock.Post((message, None));


        public StatefulReplyDataflowAgent(TState initialState,
            Func<TState, TMessage, Task<TState>> projection,
            Func<TState, TMessage, Task<(TState, TReply)>> ask,
            CancellationTokenSource cts = null)
        {
            state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ? cts.Token : CancellationToken.None
            };
            // TODO : 5.4
            // implement a state full agent that apply a function (Task projection)
            // to the incoming messages and that updates the Agent state
            // Suggestion, look the implementation of the "Ask"
            actionBlock = new ActionBlock<(TMessage, Option<TaskCompletionSource<TReply>>)>(null);
        }

        public StatefulReplyDataflowAgent(TState initialState,
            Func<TState, TMessage, TState> projection,
            Func<TState, TMessage, (TState, TReply)> ask,
            CancellationTokenSource cts = null)
        {
            state = initialState;
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts != null ? cts.Token : CancellationToken.None
            };
            // TODO : 5.4
            // implement a state full agent that apply a function (projection)
            // to the incoming messages and that updates the Agent state
            // Suggestion, look the implementation of the "Ask"
            actionBlock = new ActionBlock<(TMessage, Option<TaskCompletionSource<TReply>>)>(null);
        }
    }
}