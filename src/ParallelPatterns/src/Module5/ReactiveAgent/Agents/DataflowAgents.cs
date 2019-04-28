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
}