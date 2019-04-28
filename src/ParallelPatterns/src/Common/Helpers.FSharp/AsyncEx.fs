namespace FunctionalConcurrency

open System
open System.IO
open System.Threading.Tasks
open System.Threading

[<AutoOpen>]
module AsyncHelpers =

    // Extending the Asynchronous-Workflow to support Task<’a>
    type Microsoft.FSharp.Control.AsyncBuilder with
        member x.Bind(t : Task<'T>, f : 'T -> Async<'R>) : Async<'R> = async.Bind(Async.AwaitTask t, f)
        member x.ReturnFrom(computation : Task<'T>) = x.ReturnFrom(Async.AwaitTask computation)

        member x.Bind(t : Task, f : unit -> Async<'R>) : Async<'R> = async.Bind(Async.AwaitTask t, f)
        member x.ReturnFrom(computation : Task) = x.ReturnFrom(Async.AwaitTask computation)

        member this.Using(disp:#System.IDisposable, (f:Task<'T> -> Async<'R>)) : Async<'R> =
            this.TryFinally(f disp, fun () ->
                match disp with
                    | null -> ()
                    | disp -> disp.Dispose())

    type Microsoft.FSharp.Control.Async<'a> with
        static member Parallel2(a : Async<'a>, b : Async<'b>) : Async<'a * 'b> = async {
                let! a = a |> Async.StartChild
                let! b = b |> Async.StartChild
                let! a = a
                let! b = b
                return a, b }

        static member Parallel3(a : Async<'a>, b : Async<'b>, c : Async<'c>) : Async<'a * 'b * 'c> = async {
                let! a = a |> Async.StartChild
                let! b = b |> Async.StartChild
                let! c = c |> Async.StartChild
                let! a = a
                let! b = b
                let! c = c
                return a, b, c }

        static member ParallelWithThrottle (millisecondsTimeout : int) (limit : int) (items : 'a seq)
                        (operation : 'a -> Async<'b>) =
            let semaphore = new SemaphoreSlim(limit, limit)
            let mutable count = (items |> Seq.length)
            items
            |> Seq.map (fun item ->
                    async {
                        let! isHandleAquired = Async.AwaitTask
                                                <| semaphore.WaitAsync(millisecondsTimeout = millisecondsTimeout)
                        if isHandleAquired then
                            try
                                return! operation item
                            finally
                                if Interlocked.Decrement(&count) = 0 then semaphore.Dispose()
                                else semaphore.Release() |> ignore
                        else return! failwith "Failed to acquire handle"
                    })
            |> Async.Parallel

        /// Starts the specified operation using a new CancellationToken and returns
        /// IDisposable object that cancels the computation.
        static member StartCancelableDisposable(computation:Async<unit>) =
            let cts = new System.Threading.CancellationTokenSource()
            Async.Start(computation, cts.Token)
            { new IDisposable with member x.Dispose() = cts.Cancel() }

        static member StartContinuation (cont: 'a -> unit) (computation:Async<'a>) =
            Async.StartWithContinuations(computation,
                (fun res-> cont(res)),
                (ignore),
                (ignore))

        static member Map (map:'a -> 'b, x:Async<'a>) = async {let! r = x in return map r}

        static member Tap (action:'a -> 'b, x:Async<'a>) = Async.Map(action, x) |> Async.Ignore|> Async.Start; x

        static member AwaitObservable(observable: IObservable<'T>) =
            let tcs = new TaskCompletionSource<'T>()
            let rec observer = (fun result ->
                tcs.SetResult result
                remover.Dispose())
            and remover: IDisposable = observable.Subscribe observer
            Async.AwaitTask tcs.Task

        static member AwaitObservable(ev1:IObservable<'T1>, ev2:IObservable<'T2>) =
          List.reduce Observable.merge
            [ ev1 |> Observable.map Choice1Of2
              ev2 |> Observable.map Choice2Of2 ]
          |> Async.AwaitObservable

        static member AwaitObservable
            ( ev1:IObservable<'T1>, ev2:IObservable<'T2>, ev3:IObservable<'T3> ) =
          List.reduce Observable.merge
            [ ev1 |> Observable.map Choice1Of3
              ev2 |> Observable.map Choice2Of3
              ev3 |> Observable.map Choice3Of3 ]
          |> Async.AwaitObservable


        static member inline awaitPlainTask (task: Task) =
            // rethrow exception from preceding task if it fauled
            let continuation (t : Task) : unit =
                match t.IsFaulted with
                | true -> raise t.Exception
                | arg -> ()
            task.ContinueWith continuation |> Async.AwaitTask

        static member inline startAsPlainTask (work : Async<unit>) = Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

    type RequestGate(n:int) =
        let sem = new System.Threading.SemaphoreSlim(n, n)
        member x.Acquire(?timeout) =
            async { let! ok = Async.AwaitWaitHandle(sem.AvailableWaitHandle, ?millisecondsTimeout=timeout)
                if (ok) then
                    return
                        { new System.IDisposable with
                            member x.Dispose() =
                                sem.Release() |> ignore }
                else
                    return! failwith "Couldn't acquire Gate" }

module rec AsyncOperators =

    // ( <*> ) : f:Async<('a -> 'b)> -> m:Async<'a> -> Async<'b>
    let (<*>) = Async.apply
    // <!> : f:('a -> 'b) -> m:Async<'a> -> Async<'b>
    let (<!>) = Async.map

    let (<^>) = Async.``pure``

    // Bind
    // operation:('a -> Async<'b>) -> value:Async<'a> -> Async<'b>
    let inline (>>=) (operation:('a -> Async<'b>)) (value:Async<'a>) = async.Bind(value, operation)

    // Kliesli
    // val ( >=> ) : fAsync:('a -> Async<'b>) -> gAsync:('b -> Async<'c>) -> arg:'a -> Async<'c>
    let Kliesli (fAsync:'a -> Async<'b>) (gAsync:'b -> Async<'c>) (arg:'a) = async {
        let! f = Async.StartChild (fAsync arg)
        let! result = f
        return! gAsync result }

    let (>=>) = Kliesli

    [<RequireQualifiedAccess>]
    module Async =

        // x:'a -> Async<'a>
        let retn x = async.Return x        
        let unit x = async.Return x

        // f:('b -> Async<'c>) -> a:Async<'b> -> Async<'c>
        let bind (f:'b -> Async<'c>) (a:Async<'b>) : Async<'c> = async.Bind(a, f)

        // map:('a -> 'b) -> value:Async<'a> -> Async<'b>
        let fmap (map : 'a -> 'b) (value : Async<'a>) : Async<'b> = async.Bind(value, map >> async.Return)

        let join (value:Async<Async<'a>>) : Async<'a> = async.Bind(value, id)

        // F# async applicative functor
        let ``pure`` (value:'a) = async.Return value

        // funAsync:Async<('a -> 'b)> -> opAsync:Async<'a> -> Async<'b>
        let apply (funAsync:Async<'a -> 'b>) (opAsync:Async<'a>) = async {
            // We start both async task in Parallel
            let! funAsyncChild = Async.StartChild funAsync
            let! opAsyncChild = Async.StartChild opAsync

            let! funAsyncRes = funAsyncChild
            let! opAsyncRes = opAsyncChild
            return funAsyncRes opAsyncRes
            }

        let map (map : 'a -> 'b) (value : Async<'a>) : Async<'b> = async.Bind(value, map >> async.Return)

        // (('a -> 'b) -> Async<'a> -> Async<'b>)
        let (<!>) = map

        let lift2 (func:'a -> 'b -> 'c) (asyncA:Async<'a>) (asyncB:Async<'b>) =
            func <!> asyncA <*> asyncB

        let lift3 (func:'a -> 'b -> 'c -> 'd) (asyncA:Async<'a>) (asyncB:Async<'b>) (asyncC:Async<'c>) =
            func <!> asyncA <*> asyncB <*> asyncC

        let tee (fn:'a -> 'b) (x:Async<'a>) = (map fn x) |> Async.Ignore|> Async.Start; x

        //  Async-workflow conditional combinators
        let ifAsync (predicate:Async<bool>) funcA funcB =
            async.Bind(predicate, fun p -> if p then funcA else funcB)

        // (Context -> bool) -> Context -> Async<Context option>
        let iff predicate funcA funcB value =
            async.Bind(predicate value, fun p -> if p then funcA value else funcB value)

        let iffAsync (predicate:Async<'a -> bool>) (context:Async<'a>) = async {
            let! p = predicate <*> context
            return if p then Some context else None }


        // predicate:Async<bool> -> Async<bool>
        let inline notAsync (predicate:Async<bool>) = async.Bind(predicate, not >> async.Return)

        // funcA:Async<bool> -> funcB:Async<bool> -> Async<bool>
        let inline AND (funcA:Async<bool>) (funcB:Async<bool>) = ifAsync funcA funcB (async.Return false)

        // funcA:Async<bool> -> funcB:Async<bool> -> Async<bool>
        let inline OR (funcA:Async<bool>) (funcB:Async<bool>) = ifAsync funcA (async.Return true) funcB

        let (<&&>) funcA funcB = AND funcA funcB
        let (<||>) funcA funcB = OR funcA funcB

        let traverse f list =
            let folder x xs = retn (fun x xs -> x :: xs) <*> f x <*> xs
            List.foldBack folder list (retn [])

        let sequence seq =
            let inline cons a b = lift2 (fun x xs -> x :: xs)  a b
            List.foldBack cons seq (retn [])

        // f:('a -> Async<'b>) -> x:'a list -> Async<'b list>
        let mapM f x = sequence (List.map f x)

        // xsm:Async<#seq<'b>> * f:('b -> 'c) -> Async<seq<'c>>
        let asyncFor(operations: #seq<'a> Async, f:'a -> 'b) =
            map (Seq.map map) operations

        // x2yR:('a -> Async<'b>) -> y2zR:('b -> Async<'c>) -> ('a -> Async<'c>)
        let andCompose x2yR y2zR = x2yR >=> y2zR

        // operation without parentheses now - because it is not a normal F# function anymore, but an asynchronous workflow
        // onThreadPool function is generic and run on the ThreadPool operation is not a function of type unit -> unit
        // but also an Async<'a>, which means it returns a value of type 'a
        // That means we can not only run "fire-and-forget" style code on the ThreadPool,
        // but we can actually get results back. That sounds like a useful property for our code to have.
        let onThreadPool operation =
            async {
                let context = System.Threading.SynchronizationContext.Current
                do! Async.SwitchToThreadPool()
                let! result = operation
                do! Async.SwitchToContext context
                return result
            }


[<AutoOpen>]
module AsyncBuilderEx =

    // Extending the Asynchronous-Workflow to support Task<’a>
    type Microsoft.FSharp.Control.AsyncBuilder with
        member x.Bind(t : Task<'T>, f : 'T -> Async<'R>) : Async<'R> = async.Bind(Async.AwaitTask t, f)
        member x.ReturnFrom(computation : Task<'T>) = x.ReturnFrom(Async.AwaitTask computation)
        member x.Bind(t : Task, f : unit -> Async<'R>) : Async<'R> = x.Bind(t.ContinueWith ignore |> Async.AwaitTask, f)

        member x.ReturnFrom(computation : Task) = x.ReturnFrom(Async.AwaitTask computation)

        member this.Using(disp:#System.IDisposable, (f:Task<'T> -> Async<'R>)) : Async<'R> =
            this.TryFinally(f disp, fun () ->
                match disp with
                    | null -> ()
                    | disp -> disp.Dispose())

module AsyncHandler =

    type AsyncResult<'a> =
    | OK of 'a
    | Failure of exn
        static member ofChoice value =
            match value with
            | Choice1Of2 value -> AsyncResult.OK value
            | Choice2Of2 e -> Failure e

        static member ofOption optValue =
            match optValue with
            | Some value -> OK value
            | None -> Failure (ArgumentException())

    let handler operation = async {
        let! result = Async.Catch operation
        return
            match result with
            | Choice1Of2 result -> OK result
            | Choice2Of2 error  -> Failure error    }


    //  Implementation of mapHanlder Async-Combinator
    let mapHandler (continuation:'a -> Async<'b>)  comp = async {
        //Evaluate the outcome of the first future
        let! result = comp
        // Apply the mapping on success
        match result with
        | OK r -> return! handler (continuation r)
        | Failure e -> return Failure e
    }

    let map f =
        let map value = handler (async { return (f value) }) in mapHandler map

    let wrap (computation:Async<'a>) =
        async {
            let! choice = (Async.Catch computation)
            return (AsyncResult<'a>.ofChoice choice)
        }

    let wrapOptionAsync (computation:Async<'a option>) =
        async {
            let! choice = computation
            return (AsyncResult<'a>.ofOption choice)
        }
