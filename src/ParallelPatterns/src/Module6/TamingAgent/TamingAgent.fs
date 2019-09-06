module TamingAgentModule

open System
open System.Collections.Generic
open System.Net
open System.Text
open System.Text.RegularExpressions
open System.IO
open System.Threading
open AgentEx


// Implementation of the TamingAgent
// Message type used by the agent - contains queuing
// of work items and notification of completion
type JobRequest<'T, 'R> =
    | Ask of 'T * AsyncReplyChannel<'R>
    | Completed
    | Quit

// Implements an agent that lets you throttle the degree of parallelism by limiting the
// number of work items that are processed in parallel.
type TamingAgent<'T, 'R>(limit, operation:'T -> Async<'R>) =
    let jobCompleted = new Event<'R>()

    let tamingAgent = Agent<JobRequest<'T, 'R>>.Start(fun agent ->
        let dispose() = (agent :> IDisposable).Dispose()
        /// Represents a state when the agent is working
        let rec running jobCount = async {
          // Receive any message
          let! msg = agent.Receive()
          match msg with
          | Quit -> dispose()
          | Completed ->
              // Decrement the counter of work items
              return! running (jobCount - 1)
          // Start the work item & continue in blocked/working state
          | Ask(job, reply) ->
               do!
                 async { try
                             let! result = operation job
                             jobCompleted.Trigger result
                             reply.Reply(result)
                         finally agent.Post(Completed) }
               |> Async.StartChild |> Async.Ignore
               if jobCount <= limit - 1 then return! running (jobCount + 1)
               else return! idle ()
            /// Represents a state when the agent is blocked
            }
        and idle () =
              // Use 'Scan' to wait for completion of some work
              agent.Scan(function
              | Completed -> Some(running (limit - 1))
              | _ -> None)
        // Start in working state with zero running work items
        running 0)

    /// Queue the specified asynchronous workflow for processing
    member this.Ask(value) = tamingAgent.PostAndAsyncReply(fun ch -> Ask(value, ch))
    member this.Stop() = tamingAgent.Post(Quit)
    member x.Subscribe(action) = jobCompleted.Publish |> Observable.subscribe(action)

////////////////////////////////////////////////
//  Test Taming Agent
////////////////////////////////////////////////
let urls =
    [ "http://www.live.com";
        "http://news.live.com";
        "http://www.yahoo.com";
        "http://news.yahoo.com";
        "http://www.google.com";
        "http://news.google.com"; ]

let httpAsync(url:string) =
    async { let req = WebRequest.Create(url)
            let! resp = req.AsyncGetResponse()
            use stream = resp.GetResponseStream()
            use reader = new StreamReader(stream)
            let! http = reader.ReadToEndAsync() |> Async.AwaitTask
            printfn "Thread id %d - http len %d" Thread.CurrentThread.ManagedThreadId http.Length
            return http.Length }

let agent = TamingAgent(2, httpAsync)


urls
|> Seq.map( agent.Ask )
|> Async.Parallel
|> Async.RunSynchronously
|> Seq.sum
|> (printfn "total size %d")


let pipelineAgent l f =
    let a = TamingAgent(l, f)
    fun x -> a.Ask(x)
    
let retn x = async { return x }
    
let bind f xAsync = async {
    let! x = xAsync
    return! f x }
        
let (>>=) x f = bind f x // async.Bind(x, f)    
let pipeline agent1 agent2 x = retn x >>= agent1 >>= agent2   