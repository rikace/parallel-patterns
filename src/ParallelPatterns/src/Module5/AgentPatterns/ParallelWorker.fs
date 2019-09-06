namespace AgentPatterns

module ParallelWorker =
    open System
    open System.Threading
    
    // Parallel agent-based 
    // Agents (MailboxProcessor) provide building-block for other 
    // primitives - like parallelWorker

    // TODO 
    // implement an Agent coordinator that send
    // a message to process to a collection sub-agents (children)
    // in a round-robin fashion

    let parallelCoordinator n f =
        MailboxProcessor.Start(fun inbox ->
            let workers = Array.init n (fun i -> MailboxProcessor.Start(f))
            
            // missing code
            // create a recursive/async lopp with an
            // internal state to track the child-agent index
            // where a message was swend last
            async.Return(())
    )

    // TODO : 5.18
    // A reusable parallel worker model built on F# agents
    // implement a parallel worker based on MailboxProcessor, which coordinates the work in a Round-Robin fashion
    // between a set of children MailboxProcessor(s)
    // use an Array initializer to create the collection of MailboxProcessor(s)
    // the internal state should keep track of the index of the child to the send  the next message
    let parallelWorker n (f:_ -> Async<unit>) =
        // TODO : use the "parallelCoordinator" for the implementation
        Unchecked.defaultof<MailboxProcessor<_>>

    let tprintfn s =
        async.Return (printfn "Executing %s on thread %i" s Thread.CurrentThread.ManagedThreadId)
        
    let paralleltprintfn s = async {
        printfn "Executing %s on thread %i" s Thread.CurrentThread.ManagedThreadId
        Thread.Sleep(300)
    }
    
    let echo = parallelWorker 1 tprintfn
    let echos = parallelWorker 4 paralleltprintfn
    
    let messages = ["a";"b";"c";"d";"e";"f";"g";"h";"i";"l";"m";"n";"o";"p";"q";"r";"s";"t"]
    printfn "...Just one guy doing the work"
    messages |> Seq.iter (fun msg -> echo.Post(msg))
    Thread.Sleep 1000
    printfn "...With a little help from his friends"
    messages |> Seq.iter (fun msg -> echos.Post(msg))  

module ParallelAgentPipeline =
    open ParallelWorker
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.PixelFormats
    open System.Threading
    open ImageHandlers
    open System.IO
    open Helpers

     let images = Directory.GetFiles("../../../../../Data/paintings", "*.jpg")
    
    let imageProcessPipeline (destination:string) (imageSrc:string) = async {
        if Directory.Exists destination |> not then
            Directory.CreateDirectory destination |> ignore
            
        let imageDestination = Path.Combine(destination, Path.GetFileName(imageSrc))
        load imageSrc
        |> resize 400 400
        |> convert3D
        |> setFilter ImageFilters.Green
        |> saveImage destination }
        
    // TODO :
    //      ParallelAgentPipeline
    //      implement a reusable parallel worker model built on F# agents 
    //      complete the TODOs
    //      
    let start () = 
        let agentImage =
            parallelWorker 4 (imageProcessPipeline "../../../../../Data/paintings//Output")
        images
        |> Seq.iter(fun image -> agentImage.Post image)
