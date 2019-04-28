namespace ParallelPatterns.Fsharp
open System
open System.Threading
open System.Collections.Generic
open System.Threading.Tasks
open System.Runtime.CompilerServices

module TaskCombinators = 

    [<Extension>]
    type TaskAsCopmlete() =
    
        [<Extension>]
        static member ContinueAsComplete (input : IEnumerable<'T>, selector : Func<'T, Task<'R>>) =  
            let inputTaskList = 
                input |> Seq.map(fun i -> selector.Invoke(i)) |> Seq.toList
                
            let completionSourceList = List.init inputTaskList.Length (fun i -> TaskCompletionSource<_>())
                
            let prevIndex = ref -1
            
            // TODO 4
            let continuation =
                // Missing code
                // replace the "Unchecked.defaultof<Task<_> -> bool>"
                // with the missing implementation
                Unchecked.defaultof<Task<_> -> bool>
               
            for inputTask in inputTaskList do
                inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default) |> ignore

            completionSourceList |> Seq.map(fun source -> source.Task)
            