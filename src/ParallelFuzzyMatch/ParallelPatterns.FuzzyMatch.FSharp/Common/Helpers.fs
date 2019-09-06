namespace ParallelPatterns.Fsharp
open System
open System.Threading.Tasks
open System.Runtime.CompilerServices

[<AutoOpen>]
module ParallelPatternsFsharpHelpers =

    type Agent<'T> = MailboxProcessor<'T>

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


open System.Runtime.CompilerServices

[<Sealed; Extension>]
type public FSharpFuncUtils =

    [<Extension>]
    static member ToFSharpFunc<'a,'b> (func:System.Converter<'a,'b>) = fun x -> func.Invoke(x)

    [<Extension>]
    static member ToFSharpFunc<'a,'b> (func:System.Func<'a,'b>) = fun x -> func.Invoke(x)

    [<Extension>]
    static member ToFSharpFunc<'a,'b,'c> (func:System.Func<'a,'b,'c>) = fun x y -> func.Invoke(x,y)

    [<Extension>]
    static member ToFSharpAction<'a> (func:System.Action<'a>) = fun x -> func.Invoke(x)

    [<Extension>]
    static member ToFSharpAction<'a,'b> (func:System.Action<'a,'b>) = fun x y z -> func.Invoke(x,y)

    [<Extension>]
    static member ToFSharpAction<'a,'b,'c> (func:System.Action<'a,'b,'c>) = fun x y z -> func.Invoke(x,y, z)

    [<Extension>]
    static member ToFSharpFunc<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = fun x y z -> func.Invoke(x,y,z)

    [<Extension>]
    static member Create<'a> (func:System.Action<'a>) = FSharpFuncUtils.ToFSharpAction func

    [<Extension>]
    static member Create<'a,'b> (func:System.Action<'a,'b>) = FSharpFuncUtils.ToFSharpAction func

    [<Extension>]
    static member Create<'a,'b> (func:System.Func<'a,'b>) = FSharpFuncUtils.ToFSharpFunc func

    [<Extension>]
    static member Create<'a,'b,'c> (func:System.Func<'a,'b,'c>) = FSharpFuncUtils.ToFSharpFunc func

    [<Extension>]
    static member Create<'a,'b,'c,'d> (func:System.Func<'a,'b,'c,'d>) = FSharpFuncUtils.ToFSharpFunc func
