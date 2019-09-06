namespace AgentEx

open System
open System.Threading
open System.Threading.Tasks

open System
open System.Threading
open System.Threading.Tasks

module AgentTypes =

    type IAsyncReplyChannel<'a> =
        abstract Reply : 'a -> unit

    type MailboxReplyChannel<'a>(asyncReplyChannel:AsyncReplyChannel<'a>) =
        interface IAsyncReplyChannel<'a> with
            member x.Reply(msg) = asyncReplyChannel.Reply(msg)

    type ReplyChannel<'a>() =
         let tcs = new TaskCompletionSource<'a>()

         member x.WaitResult = async {
                return! tcs.Task |> Async.AwaitTask
            }

         interface IAsyncReplyChannel<'a> with
             member x.Reply(msg) = tcs.SetResult(msg)


module AgentSystem =
    open AgentTypes

    [<AbstractClass>]
    type AgentRef(id:string) =
        member val Id = id with get, set
        abstract Start : unit -> unit
        
    [<AbstractClass>]
    type AgentRef<'a>(id:string) =
        inherit AgentRef(id)
        abstract Receive : unit -> Async<'a>
        abstract Post : 'a -> unit
        abstract PostAndTryAsyncReply : (IAsyncReplyChannel<'b> -> 'a) -> Async<'b option>      
        
        
    type Agent<'a>(id:string, comp, ?token) =
        inherit AgentRef<'a>(id)
        let mutable agent = Unchecked.defaultof<MailboxProcessor<'a>>

        override x.Post(msg:'a) = agent.Post(msg)
        override x.PostAndTryAsyncReply(builder) = agent.PostAndTryAsyncReply(fun rc -> builder(new MailboxReplyChannel<_>(rc)))

        override x.Receive() = agent.Receive()
        override x.Start() =
            agent <- MailboxProcessor.Start((fun inbox -> comp (x :> AgentRef<_>)), ?cancellationToken = token)     
            
    [<RequireQualifiedAccess>]        
    module Registry =
    
        let mutable private agents = Map.empty<string, AgentRef list>
    
        let register (ref:AgentRef<'a>) =
            match Map.tryFind ref.Id agents with
            | Some(refs) ->
                agents <- Map.add ref.Id ((ref :> AgentRef) :: refs) agents
            | None ->
                agents <- Map.add ref.Id [ref :> AgentRef] agents
            ref
    
        let resolveOne id =
            match Map.find id agents with
            | [h] -> h
            | _ -> failwithf "Found multiple agents (%s)" id
    
        let resolve id =
            Map.find id agents             
            
    [<RequireQualifiedAccess>]            
    module AgentRef =
    
        type Replyable<'a, 'b> = | Reply of 'a * IAsyncReplyChannel<'b>
        
        let start (ref:AgentRef<'a>) =
            ref.Start()
            ref
    
        let spawn ref =
            Registry.register ref
            |> start
    
        let ref (ref:AgentRef<'a>) = ref :> AgentRef
    
        let post (refs:#seq<AgentRef>) (msg:'a) =
            refs |> Seq.iter (fun r -> (r :?> AgentRef<'a>).Post(msg))
    
        let postAndTryAsyncReply (refs:#seq<AgentRef>) msg =
            refs
            |> Seq.map (fun r -> (r :?> AgentRef<'a>).PostAndTryAsyncReply(msg))
            |> Async.Parallel
    
        let postAndAsyncReply (refs:#seq<AgentRef>) msg =
            async {
                let! responses = postAndTryAsyncReply refs msg
                return responses |> Seq.choose id
            }
    
        let postAndReply (refs:#seq<AgentRef>) msg =
            postAndAsyncReply refs msg |> Async.RunSynchronously
    
        let resolve id = Registry.resolve id            
        
        let pipelined (agent:AgentRef<_>) previous =
            agent.Id, async {
                let! result = agent.PostAndTryAsyncReply(fun rc -> Reply(previous,rc))
                match result with
                | Some(result) ->
                    match result with
                    | Choice1Of2(result) -> return result
                    | Choice2Of2(err) -> return raise(err)
                | None -> return failwithf "Stage timed out %s: failed" agent.Id
            }        