module FunConcurrency.Async


open System
open System.Threading
open System.Threading.Tasks
open FunConcurrency

module AsyncOperators =
    [<RequireQualifiedAccess>]
    module Async =

        // x:'a -> Async<'a>
        let retn x = async.Return x

        // f:('b -> Async<'c>) -> a:Async<'b> -> Async<'c>
        let bind (f:'b -> Async<'c>) (a:Async<'b>) : Async<'c> = async.Bind(a, f)

        // map:('a -> 'b) -> value:Async<'a> -> Async<'b>
        let fmap (map : 'a -> 'b) (value : Async<'a>) : Async<'b> = async.Bind(value, map >> async.Return)

        let join (value:Async<Async<'a>>) : Async<'a> = async.Bind(value, id)

        // async applicative functor
        let ``pure`` (value:'a) = async.Return value


        let run continuation op = Async.StartWithContinuations(op, continuation, (ignore), (ignore))

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

        let kleisli (f:'a -> Async<'b>) (g:'b -> Async<'c>) (x:'a) = bind g (f x)

        let lift2 (func:'a -> 'b -> 'c) (asyncA:Async<'a>) (asyncB:Async<'b>) =
            apply (map func asyncA)  asyncB

        let tee (fn:'a -> 'b) (x:Async<'a>) = (map fn x) |> Async.Ignore|> Async.Start; x

        //  Async-workflow conditional combinators
        let ifAsync (predicate:Async<bool>) funcA funcB =
            async.Bind(predicate, fun p -> if p then funcA else funcB)

        let notAsync predicate = async.Bind(predicate, not >> async.Return)

        let iffAsync (predicate:Async<'a -> bool>) (context:Async<'a>) = async {
            let! p = apply predicate context
            return if p then Some context else None }

        let AND funcA funcB = ifAsync funcA funcB (async.Return false)
        let OR funcA funcB = ifAsync funcA (async.Return true) funcB

       // let (<&&>) funcA funcB = AND funcA funcB
       // let (<||>) funcA funcB = OR funcA funcB

        let sequence seq =
            let inline cons a b = lift2 (fun x xs -> x :: xs)  a b
            List.foldBack cons seq (retn [])

        // f:('a -> Async<'b>) -> x:'a list -> Async<'b list>
        let mapM f x = sequence (List.map f x)

    [<AutoOpen>]
    module AsyncOperators =
        // ( <*> ) : f:Async<('a -> 'b)> -> m:Async<'a> -> Async<'b>
        let (<*>) = Async.apply
        // <!> : f:('a -> 'b) -> m:Async<'a> -> Async<'b>
        let (<!>) = Async.map

        let (<^>) = Async.``pure``

        // Bind
        // operation: value:Async<'a> -> ('a -> Async<'b>) -> Async<'b>
        let inline (>>=) (item:Async<'a>) (operation:'a -> Async<'b>) = Async.bind operation item

        // Kliesli
        // val ( >=> ) : fAsync:('a -> Async<'b>) -> gAsync:('b -> Async<'c>) -> arg:'a -> Async<'c>
        let (>=>) (fAsync:'a -> Async<'b>) (gAsync:'b -> Async<'c>) (arg:'a) = async {
            let! f = Async.StartChild (fAsync arg)
            let! result = f
            return! gAsync result }
