namespace DirectGraphDependencies


    open System
    open System.Collections.Generic
    open System.Threading
    open Microsoft.FSharp.Collections
    open System.Threading.Tasks

    type MList<'a> = System.Collections.Generic.List<'a>
       
    // DAG F# Agent to parallelize the execution of operations with dependencies
    type TaskMessage =  
        | AddTask of int * TaskInfo
        | QueueTask of TaskInfo
        | ExecuteTasks
    (**F# The Type TaskInfo contains and keeps track of the details of the registered task, the id, 
    function operation and dependency edges. The execution context is captured to be able to access
    information during the delayed execution such as the current user, any state associated
    with the logical thread of execution, code-access security information, and so forth. 
    The start and end for the execution time will be published when the event fires.**)
    and TaskInfo =  
        { Context : System.Threading.ExecutionContext
          Edges : int array
          Id : int
          Task : Func<Task>
          EdgesLeft : int option
          Start : DateTimeOffset option
          End : DateTimeOffset option }

    type ParallelTasksDAG() as this =

        let onTaskCompleted = new Event<TaskInfo>() 

        let verifyThatAllOperationsHaveBeenRegistered (tasks:Dictionary<int, TaskInfo>) =
            let tasksNotRegistered =
                tasks.Values
                |> (Seq.collect (fun f -> f.Edges) >> set)
                |> Seq.filter(tasks.ContainsKey >> not)
            if tasksNotRegistered |> Seq.length > 0 then
                let edgesMissing = tasksNotRegistered |> Seq.map (string) |> Seq.toArray
                raise (InvalidOperationException(sprintf "Missing operation: %s" (String.Join(", ", edgesMissing))))

        let verifyTopologicalSort(tasks:Dictionary<int, TaskInfo>) =
            // Build up the dependencies graph
            let tasksToFrom = new Dictionary<int, MList<int>>(tasks.Values.Count, HashIdentity.Structural)
            let tasksFromTo = new Dictionary<int, MList<int>>(tasks.Values.Count, HashIdentity.Structural)

            for op in tasks.Values do
                // Note that op.Id depends on each of op.Edges
                tasksToFrom.Add(op.Id, new MList<int>(op.Edges))
                // Note that each of op.Dependencies is relied on by op.Id
                for deptId in op.Edges do
                    let success, _ = tasksFromTo.TryGetValue(deptId)
                    if not <| success then tasksFromTo.Add(deptId, new MList<int>())
                    tasksFromTo.[deptId].Add(op.Id)
            // Create the sorted list
            let partialOrderingIds = new MList<int>(tasksToFrom.Count)
            let iterationIds = new MList<int>(tasksToFrom.Count)

            let rec buildOverallPartialOrderingIds() =
                match tasksToFrom.Count with
                | 0 -> Some(partialOrderingIds)
                | _ ->  iterationIds.Clear()
                        for item in tasksToFrom do
                            if item.Value.Count = 0 then
                                iterationIds.Add(item.Key)
                                let success, depIds = tasksFromTo.TryGetValue(item.Key)
                                if success = true then
                                    // Remove all outbound edges
                                    for depId in depIds do
                                        tasksToFrom.[depId].Remove(item.Key) |> ignore
                        // If nothing was found to remove, there's no valid sort.
                        if iterationIds.Count = 0 then None
                        else
                            // Remove the found items from the dictionary and
                            // add them to the overall ordering
                            for id in iterationIds do
                                tasksToFrom.Remove(id) |> ignore
                            partialOrderingIds.AddRange(iterationIds)
                            buildOverallPartialOrderingIds()
            buildOverallPartialOrderingIds()

        let verifyThereAreNoCycles(operations:Dictionary<int, TaskInfo>) =
            if verifyTopologicalSort(operations) = None then
                raise (InvalidOperationException("Cycle detected"))

        let nrd = function
            | Some(n) -> Some(n - 1)
            | None -> None

        let rec getDependentOperation (dep : int list) (ops : Dictionary<int, TaskInfo>) acc =
            match dep with
            | [] -> acc
            | h :: t ->     ops.[h] <- { ops.[h] with EdgesLeft = nrd ops.[h].EdgesLeft }
                            match ops.[h].EdgesLeft.Value with
                            | 0 ->  getDependentOperation t ops (ops.[h] :: acc)
                            | _ ->  getDependentOperation t ops acc
        (**The core of the solution is implemented using a MailboxProcessor (aka Agent) which provides several benefits. 
        Because the natural thread-safety of this is primitive,
        I can use .NET mutable collection to simplify the implementation of DAG. 
        Immutability is an important component for writing correct and lock-free concurrent applications. 
        Another important component to reach a thread safe result is isolation. 
        The MailboxProcessor provides both concepts out of the box. In this case, we are taking advantage of isolation.**)
        let dagAgent =
            let inbox = new MailboxProcessor<TaskMessage>(fun inbox ->
                (**The MailboxProcessor named dagAgent is keeping the registered tasks 
                   in a current state “tasks” which is a map (tasks : Dictionary<int, TaskInfo>) 
                   between the id of each task and its details. **)
                (**Agent also keeps the state of the edge dependencies for each task id (edges : Dictionary<int, int list>). 
                   When the Agent receives the notification to start the execution, 
                   part of the process involves verifying that all the edge dependencies are registered and that there are no**)
                let rec loop (tasks : Dictionary<int, TaskInfo>) 
                             (edges : Dictionary<int, int list>) = async {
                        let! msg = inbox.Receive()
                        match msg with
                        | ExecuteTasks -> 
                            // If any of the validations are not satisfied, the process is interrupted and an error is thrown. 
                            // Verify that all operations are registered
                            verifyThatAllOperationsHaveBeenRegistered(tasks)
                            // Verify no cycles
                            verifyThereAreNoCycles(tasks)

                            let fromTo = new Dictionary<int, int list>()
                            let ops = new Dictionary<int, TaskInfo>()

                            // Fill dependency data structures
                            for KeyValue(key, value) in tasks do 
                                let operation =
                                    { value with EdgesLeft = Some(value.Edges.Length) }
                                for from in operation.Edges do
                                    let exists, lstDependencies = fromTo.TryGetValue(from)
                                    if not <| exists then
                                        fromTo.Add(from, [ operation.Id ])
                                    else
                                        fromTo.[from] <- (operation.Id :: lstDependencies)
                                ops.Add(key, operation)


                            ops |> Seq.iter (fun kv ->
                                                   match kv.Value.EdgesLeft with
                                                   | Some(n) when n = 0 -> inbox.Post(QueueTask(kv.Value))
                                                   | _ -> ())
                            return! loop ops fromTo
                        (**If the validation passed successfully, the process starts the execution, 
                           checking each task for dependencies thus enforcing the order and prioritization of execution. 
                           In this last case the edge task is re-queued into the dagAgent using the “QueueTask” message. 
                           Upon completion of a task, we simply remove the task from the graph. 
                           This frees up all its dependencies to be executed.**)
                        | QueueTask(taskInfo) -> 
                                Async.Start <| async {
                                    // Time and run the operation's delegate
                                    let start = DateTimeOffset.Now
                                    match taskInfo.Context with
                                    | null -> do! taskInfo.Task.Invoke() |> Async.AwaitTask
                                    | ctx ->
                                        ExecutionContext.Run(ctx.CreateCopy(),  
                                                                (fun op -> let opCtx = (op :?> TaskInfo)
                                                                           opCtx.Task.Invoke().ConfigureAwait(false) |> ignore
                                                                           ), taskInfo)
                                    let end' = DateTimeOffset.Now
                                    // Raise the operation completed event
                                    onTaskCompleted.Trigger  { taskInfo with Start = Some(start)
                                                                             End = Some(end') }  

                                    // Queue all the operations that depend on the completion
                                    // of this one, and potentially launch newly available
                                    let exists, deps = edges.TryGetValue(taskInfo.Id)
                                    if exists && deps.Length > 0 then
                                        let depOps = getDependentOperation deps tasks []
                                        edges.Remove(taskInfo.Id) |> ignore
                                        depOps |> Seq.iter (fun nestedOp -> inbox.Post(QueueTask(nestedOp))) }
                                return! loop tasks edges

                        | AddTask(id, taskInfo) -> tasks.Add(id, taskInfo) 
                                                   return! loop tasks edges
                    }
                loop (new Dictionary<int, TaskInfo>(HashIdentity.Structural)) (new Dictionary<int, int list>(HashIdentity.Structural)))
            inbox.Error |> Observable.add(fun ex -> printfn "Error : %s" ex.Message )
            inbox.Start()
            inbox

        // The event OnTaskCompleted triggered each time a task is completed providing details such as execution time.
        member this.OnTaskCompleted = onTaskCompleted.Publish |> Observable.map(id)  

        member this.ExecuteTasks() = dagAgent.Post ExecuteTasks  

        (**The purpose of the function AddTask is to register a task including arbitrary dependency edges. 
        This function accepts a unique id, a function task that has to be executed 
        and a set of edges which are representing the ids of other registered tasks 
        which all must all be completed before the current task can executed. 
        If the array is empty, it means there are no dependencies.**)
        member this.AddTask(id, task:Func<Task>, [<ParamArray>] edges : int array) =
            let data =
                { Context = ExecutionContext.Capture()
                  Edges = edges
                  Id = id
                  Task = task
                  EdgesLeft = None
                  Start = None
                  End = None }
            dagAgent.Post(AddTask(id, data))  
