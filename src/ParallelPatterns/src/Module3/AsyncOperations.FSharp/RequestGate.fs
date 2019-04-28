module RequestGate
open System.Threading

type RequestGate(n:int) =
    let semaphore = new System.Threading.SemaphoreSlim(n,n)
    member x.Aquire(?timeout) =
        async { let! ok = Async.AwaitWaitHandle(semaphore.AvailableWaitHandle, ?millisecondsTimeout=timeout)
                if ok then return { new System.IDisposable with
                                        member x.Dispose() =
                                            semaphore.Release() |> ignore }
                else return! failwith "Handle not aquired" }


