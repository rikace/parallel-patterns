namespace Pipeline.FSharp

open System
open System.Linq
open System.Collections.Concurrent
open System.Threading
open System.Threading.Tasks

// This is a version of the pipeline that uses the .NET Func delegate
// in place of the F# functions.
// This implementation is more C# friendly, because it does not require to
// convert the F# functions to Func<> using the helper extension method .ToFunc()
module PipelineFunc =

    open FunctionalHelpers

    // The IPipeline interface
    [<Interface>]
    type IPipeline<'a,'b> =  
        abstract member Then : (Func<'b, 'c>) -> IPipeline<'a, 'c>   
        abstract member Enqueue : 'a * (Func<('a * 'b), unit>) -> unit  
        abstract member Execute : int * CancellationToken -> IDisposable   
        abstract member Stop : unit -> unit  


    [<Struct>]
    type internal Continuation<'a, 'b>(input:'a, callback:Func<('a * 'b), unit>) =
        member this.Input with get() = input
        member this.Callback with get() = callback   

    //  The Parallel Functional Pipeline pattern
    type Pipeline<'a, 'b> private (func:Func<'a, 'b>) as this =
        let continuations = Array.init 3 (fun _ -> new BlockingCollection<Continuation<'a,'b>>(100))    

        let then' (nextFunction:Func<'b,'c>) =
            Pipeline(func.Compose(nextFunction)) :> IPipeline<_,_>    

        // TODO (3.b)
        let enqueue (input:'a) (callback:Func<('a * 'b), unit>) =
            // missing code
            ()


        let stop() = for continuation in continuations do continuation.CompleteAdding() 

        let execute blockingCollectionPoolSize (cancellationToken:CancellationToken) =

            cancellationToken.Register(Action(stop)) |> ignore   

            for i = 0 to blockingCollectionPoolSize - 1 do
                Task.Factory.StartNew(fun ( )->  
                    while (not <| continuations.All(fun bc -> bc.IsCompleted)) && (not <| cancellationToken.IsCancellationRequested) do
                        
                    // TODO (3.c)
                    // step to implement 
                        // 1 - take an item from the continuations collection 
                        // 2 - process the "continuation" function
                        //     Keep in mind that the continutaion function has both the
                        //     value and the callback 
                      
                    // let continuation = ref Unchecked.defaultof<Continuation<_,_>>
                    ()
                ,cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default) |> ignore

        static member Create(func:Func<'a, 'b>) =
            Pipeline(func) :> IPipeline<_,_>  

        interface IPipeline<'a, 'b> with
            member this.Then(nextFunction) = then' nextFunction
            member this.Enqueue(input, callback) = enqueue input callback |> ignore
            member this.Stop() = stop()
            member this.Execute (blockingCollectionPoolSize, cancellationToken) =
                execute blockingCollectionPoolSize cancellationToken
                { new IDisposable with member self.Dispose() = stop() }
