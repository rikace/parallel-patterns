namespace AgentPatterns

open System
open System.Collections.Generic
open AgentEx

module ParDegree =
    open System.Threading

    let par (degree: int) (f: 'a -> Async<'b>) : 'a -> Async<'b> =
        let agent = Agent.Start <| fun mailbox -> 
            async {        
                use semaphore = new SemaphoreSlim(degree)
                let rec loop () = 
                    async {
                        let! (x: 'a, rep: AsyncReplyChannel<'b>) = mailbox.Receive()
                        do! semaphore.WaitAsync() |> Async.AwaitTask
                        async {
                            try
                                let! r = f x
                                rep.Reply r
                            finally
                                semaphore.Release() |> ignore
                        } |> Async.Start
                        return! loop () 
                    }
                return! loop () 
            }
        fun x -> agent.PostAndAsyncReply(fun repCh -> x, repCh)
