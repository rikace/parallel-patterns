namespace FSharpWebCrawler
open System
open System.Threading.Tasks
open System.Runtime.CompilerServices

[<AutoOpen>]
module Helpers =
    open System.Text.RegularExpressions
    open System.Threading
    open System.Threading.Tasks

    type Agent<'T> = MailboxProcessor<'T>

    let (<--) (m:Agent<_>) msg = m.Post msg
    let (<->) (m:Agent<_>) msg = m.PostAndReply(fun replyChannel -> msg replyChannel)
    let (<-!) (m: Agent<_>) msg = m.PostAndAsyncReply(fun replyChannel -> msg replyChannel)

    let synchronize f =
      let ctx = System.Threading.SynchronizationContext.Current
      f (fun g arg ->
        let nctx = System.Threading.SynchronizationContext.Current
        if ctx <> null && ctx <> nctx then ctx.Post((fun _ -> g(arg)), null)
        else g(arg) )

    [<RequireQualifiedAccess>]
    module Async =
        let inline awaitPlainTask (task: Task) =
            let continuation (t : Task) : unit =
                match t.IsFaulted with
                | true -> raise t.Exception
                | arg -> ()
            task.ContinueWith continuation |> Async.AwaitTask

        let inline startAsPlainTask (work : Async<unit>) =
            Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

    let linkPat = "href=\s*\"[^\"h]*(http://[^&\"]*)\""
    let getLinks (txt:string) =
        [ for m in Regex.Matches(txt,linkPat)  -> m.Groups.Item(1).Value ]

    // A type that helps limit the number of active web requests
    type RequestGate(n:int) =
        let semaphore = new Semaphore(initialCount=n,maximumCount=n)
        member x.AsyncAcquire(?timeout) =
            async {
                let! ok = Async.AwaitWaitHandle(semaphore,
                                                ?millisecondsTimeout=timeout)
                if ok then
                   return
                     { new System.IDisposable with
                         member x.Dispose() =
                             semaphore.Release() |> ignore }
                else
                   return! failwith "couldn't acquire a semaphore"
            }
