namespace FunctionalConcurrency

open System.Threading.Tasks
open FunctionalConcurrency.AsyncOperators

type Result<'TSuccess> = Result<'TSuccess, exn>

//  AsyncResult handler to catch and wrap asynchronous computation
module Result =
    let ofChoice value =              
        match value with
        | Choice1Of2 value -> Ok value
        | Choice2Of2 e -> Error e

    let apply fRes xRes =
        match fRes,xRes with
        | Ok f, Ok x -> Ok (f x)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    let defaultValue value result =
        match result with
        | Ok(res) -> res
        | Error(_) -> value

    let bimap success failure =
        function
        | Ok v -> success v
        | Error x -> failure x


type AsyncResult<'a> = Async<Result<'a, exn>>

module AsyncResult =
    open System.Threading.Tasks
    open System.Threading

    let handler (operation:Async<'a>) : AsyncResult<'a> = async {
        let! result = Async.Catch operation
        return (Result.ofChoice result) }

    //  Higher order function extending the AsyncResult type
    let retn (value:'a) : AsyncResult<'a> =  value |> Ok |> async.Return

    let map (selector : 'a -> 'b) (asyncResult : AsyncResult<'a>)  = async {
        let! result = asyncResult
        match result with
        | Ok x -> return selector x |> Ok
        | Error err -> return (Error err)   }

    let mapChoice (f:'a -> Result<'b>) (a:AsyncResult<'a>) : AsyncResult<'b> =
        a |> Async.map (function
            | Ok a' -> f a'
            | Error e -> Error e)

    let bindChoice (f:'a -> AsyncResult<'b>) (a:AsyncResult<'a>) : AsyncResult<'b> =
            a |> Async.bind (function
              | Ok a' -> f a'
              | Error e ->  Error e |> async.Return)

    // computations:seq<Async<'a>> -> Async<Result<'a,exn> []>
    let parallelCatch computations  =
        computations
        |> Seq.map Async.Catch
        |> Seq.map (Async.map Result.ofChoice)
        |> Async.Parallel


    let bind (selector : 'a -> AsyncResult<'b>) (asyncResult : AsyncResult<'a>) = async {
        let! result = asyncResult
        match result with
        | Ok x -> return! selector x
        | Error err -> return Error err    }

    let bimap success failure operation = async {
        let! result = operation
        match result with
        | Ok v -> return! success v |> handler
        | Error x -> return! failure x |> handler }

    let apply (ap : AsyncResult<'a -> 'b>) (asyncResult : AsyncResult<'a>) : AsyncResult<'b> = async {
        let! result = asyncResult |> Async.StartChild
        let! fap = ap |> Async.StartChild
        let! fapResult = fap
        let! fResult = result
        match fapResult, fResult with
        | Ok ap, Ok result -> return ap result |> Ok
        | Error err, _
        | _, Error err -> return Error err    }


    let defaultValue value =
        Async.map (Result.defaultValue value)

    let inline EITHER (funcA:AsyncResult<'a>) (funcB:AsyncResult<'a>) : AsyncResult<'a> =
        let tcs = TaskCompletionSource()
        let reportResult =
            let counter = ref 0
            (fun (func:AsyncResult<'a>) ->
                async {
                    let! result = func
                    match result with
                    | Ok (x) -> tcs.TrySetResult(Ok x) |> ignore
                    | Error(e) ->
                        if !counter = 0
                        then Interlocked.Increment(counter) |> ignore
                        else tcs.SetResult(Error e)
                })

        [funcA; funcB]
        |> List.map reportResult
        |> Async.Parallel
        |> Async.StartChild
        |> ignore

        Async.AwaitTask tcs.Task

type AsyncResultBuilder()=
    member this.Return m = AsyncResult.retn m
    member this.Bind (m, f:'a -> AsyncResult<'b>) = AsyncResult.bind f m
    member this.Bind (m:Task<'a>, f:'a -> AsyncResult<'b>) = AsyncResult.bind f (m |> Async.AwaitTask |> AsyncResult.handler)
    member this.Bind (m:Task, f) = AsyncResult.bind f (m |> Async.AwaitTask |> AsyncResult.handler)
    member this.ReturnFrom m = m
    member this.Combine (funcA:AsyncResult<'a>, funcB:AsyncResult<'a>) = async {
        let! a = funcA
        match a with
        | Ok _ -> return a
        | _ -> return! funcB }

    member this.Zero() = this.Return()
    member this.Delay(f : unit -> AsyncResult<'a>) : AsyncResult<'a> = async.Delay(f)
    member this.Yield(x) = x |> async.Return |> AsyncResult.handler
    member this.YieldFrom(m) = m

    member this.Using(resource : 'T when 'T :> System.IDisposable, binder : 'T -> AsyncResult<'a>) : AsyncResult<'a> =
        async.Using(resource, binder)

[<AutoOpen>]
module AsyncResultBuilder =
    let asyncResult = AsyncResultBuilder()

    let (<!>) = AsyncResult.map
    let (<*>) = AsyncResult.apply

[<AutoOpen>]
module AsyncResultCombinators =

    let inline AND (funcA:AsyncResult<'a>) (funcB:AsyncResult<'a>) : AsyncResult<_> =
        asyncResult {
                let! a = funcA
                let! b = funcB
                return (a, b)
        }
    let inline OR (funcA:AsyncResult<'a>) (funcB:AsyncResult<'a>) : AsyncResult<'a> =
        asyncResult {
            return! funcA
            return! funcB
        }

    // funcA:AsyncResult<'a> -> funcB:AsyncResult<'a> -> AsyncResult<'a * 'a>
    let (<&&>) (funcA:AsyncResult<'a>) (funcB:AsyncResult<'a>) = AND funcA funcB
    // funcA:AsyncResult<'a> -> funcB:AsyncResult<'a> -> AsyncResult<'a>
    let (<||>) (funcA:AsyncResult<'a>) (funcB:AsyncResult<'a>) = OR funcA funcB

    let gt value (ar:AsyncResult<'a>) =
        asyncResult {
            let! result = ar
            return result > value
        }

    let (<|||>) (funcA:AsyncResult<bool>) (funcB:AsyncResult<bool>) =
        asyncResult {
            let! rA = funcA
            match rA with
            | true -> return! funcB
            | false -> return false
        }

    let (<&&&>) (funcA:AsyncResult<bool>) (funcB:AsyncResult<bool>) =
        asyncResult {
            let! (rA, rB) = funcA <&&> funcB
            return rA && rB
        }