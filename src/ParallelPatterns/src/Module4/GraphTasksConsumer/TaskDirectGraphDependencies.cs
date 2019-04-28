using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace GraphTasksConsumer
{
    // TODO 
    // (1) Make the code lock free
    //  The Execute method and the AddOperation are not thread-safe with respect to each other, 
    //  meaning that these methods are expected to be called sequentially, 
    //  first using AddOperation to add all of the operations to execute, and then calling Execute.
    //  As a result, notice that AddOperation accesses the dictionaries 
    //  without any locks or other mechanism for shared state synchronization.
    //  However, Execute does wrap part of its body in a lock. 
    //  This is because once it's queued up the first work item to run, that work item may access 
    //  and modify the dictionaries concurrently with the rest of the Execute body. 
    //  To prevent such races from happening, the rest of the body of Execute is protected. 
    //  When all relevant work has been queued, the Execute method blocks waiting for all of the work to complete.

    // (2) Complete the misisng part
    // (3) Bonus implemet these options
    // a. For operations with no dependencies, 
    //    you may want those to be able to start running as soon as they're added to the TaskDirectGraphDependencies, 
    //    rather than having to wait until Execute is called.
    // b. You may want operations to be able to return data that is passed along to dependencies, hence building up large dataflow networks.


    public interface ITaskDirectGraphDependencies
    {
        void AddOperation(int id, Action operation, params int[] dependencies);
        event EventHandler<OperationCompletedEventArgs>
          OperationCompleted;
        void Execute();
    }

    // The information about each registered operation is stored in an OperationData class
    // The Id, Operation, and Dependencies fields store the corresponding data passed into the AddOperation method as parameters
    internal class OperationData
    {
        internal int Id;
        internal Action Operation;
        internal int[] Dependencies;
        internal ExecutionContext Context;
        internal int NumRemainingDependencies;
        internal DateTimeOffset Start, End;
    }

    public class OperationCompletedEventArgs : EventArgs
    {
        internal OperationCompletedEventArgs(
          int id, DateTimeOffset start, DateTimeOffset end)
        {
            Id = id; Start = start; End = end;
        }
        public int Id { get; private set; }
        public DateTimeOffset Start { get; private set; }
        public DateTimeOffset End { get; private set; }
    }

    public class TaskDirectGraphDependencies : ITaskDirectGraphDependencies
    {
        // it keeps track of the start and end time for this operation, and these will be reported to a consumer of TaskDirectGraphDependencies 
        private Dictionary<int, OperationData> _operations = new Dictionary<int, OperationData>();
        private Dictionary<int, List<int>> _dependenciesFromTo;

        private object _stateLock = new object();
        private ManualResetEvent _doneEvent;
        private int _remainingCount;
        // The OperationCompleted event is raised any time one of the constituent operations completes, 
        // providing the ID of the operation as well as the     starting and ending times of its execution.  
        public event EventHandler<OperationCompletedEventArgs>
         OperationCompleted;

        // The main entry point into the type is the AddOperation method, 
        // which accepts a unique ID you provide to represent an operation, 
        // the delegate to be executed for that operation, 
        // and a set of dependencies upon whose completion this operation relies. 
        // If an operation has no dependencies, the parameter array will be empty.       
        public void AddOperation(
        int id, Action operation, params int[] dependencies)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");
            if (dependencies == null)
                throw new ArgumentNullException("dependencies");

            // After verifying that the arguments are valid, 
            // the method captures all of the data into an instance 
            // of the OperationData class, and that instance gets stored into the dictionary tracking all operations.
            var data = new OperationData
            {
                Context = ExecutionContext.Capture(),
                Id = id,
                Operation = operation,
                Dependencies = dependencies
            };
            // TODO : add thread safe access
            _operations.Add(id, data);
        }

        // the Execute method kicks off the process and waits for all of the operations to complete.
        // While the TaskDirectGraphDependencies class will be running operations in parallel,
        // the API itself is not meant to be thread-safe, 
        // meaning that only one thread should be used to access AddOperation and Execute at a time.
        public void Execute()
        {

            _dependenciesFromTo = new Dictionary<int, List<int>>();
            foreach (var op in _operations.Values)
            {
                op.NumRemainingDependencies = op.Dependencies.Length;

                foreach (var from in op.Dependencies)
                {
                    List<int> toList;
                    if (!_dependenciesFromTo.TryGetValue(from, out toList))
                    {
                        toList = new List<int>();
                        _dependenciesFromTo.Add(from, toList);
                    }
                    toList.Add(op.Id);
                }
            }

            // Launch and wait
            _remainingCount = _operations.Count;
            using (_doneEvent = new ManualResetEvent(false))
            {
                // TODO : add thread safe access
                lock (_stateLock)
                {
                    foreach (var op in _operations.Values)
                    {
                        if (op.NumRemainingDependencies == 0)
                            QueueOperation(op);
                    }
                }
                _doneEvent.WaitOne();
            }
        }

        private void QueueOperation(OperationData data)
        {
            // TODO
            //      add missing code
            //      this operation queue the work to run in ProcessOperation
        }
        private void ProcessOperation(OperationData data)
        {
            // Time and run the operation's delegate
            data.Start = DateTimeOffset.Now;
            if (data.Context != null)
            {
                ExecutionContext.Run(data.Context.CreateCopy(),
                  op => ((OperationData)op).Operation(), data);
            }
            else data.Operation();
            data.End = DateTimeOffset.Now;

            // Raise the operation completed event
            OnOperationCompleted(data);

            // Signal to all that depend on this operation of its
            // completion, and potentially launch newly available
            // TODO : make thread safe
            lock (_stateLock)
            {
                List<int> toList;
                // Once the operation has completed, you need to notify any dependent operations.
                // The handy_dependenciesFromTo dictionary comes into play. You simply enumerate 
                // all of the operations that depend on this particular one, look up their OperationData instances, 
                // and decrement their NumRemainingDependencies by one.If their number of remaining dependences
                // becomes zero, they're now eligible for execution, and QueueOperation is used to launch each eligible operation. 
                if (_dependenciesFromTo.TryGetValue(data.Id, out toList))
                {
                    foreach (var targetId in toList)
                    {
                        OperationData targetData = _operations[targetId];
                        if (--targetData.NumRemainingDependencies == 0)
                            QueueOperation(targetData);
                    }
                }
                _dependenciesFromTo.Remove(data.Id);

                // notify the TaskDirectGraphDependencies that one more operation has completed, 
                // potentially setting the_doneEvent(to wake up the Execute method) if it's the last operation.
                if (--_remainingCount == 0) _doneEvent.Set();
            }
        }

        private void OnOperationCompleted(OperationData data)
        {
            var handler = OperationCompleted;
            handler?.Invoke(this, new OperationCompletedEventArgs(
                data.Id, data.Start, data.End));
        }
    }
}