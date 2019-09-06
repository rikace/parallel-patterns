namespace Pipeline.FSharp.TTTT

open System.Runtime.CompilerServices
open System.Threading.Tasks

//  Task Extension in F# to enable Task LINQ-style operators
[<Sealed; Extension; CompiledName("TaskEx")>]
type TaskExtensions =
    // 'T -> M<'T>
    static member Return value : Task<'T> = Task.FromResult<'T> (value) 

    // M<'T> * ('T -> M<'U>) -> M<'U>
    static member Bind (input : Task<'T>, binder : 'T -> Task<'U>) : Task<'U> = 
        let tcs = new TaskCompletionSource<'U>()    
        
        // missing code

        tcs.Task

    static member Select (task : Task<'T>, selector : 'T -> 'U) : Task<'U> =
        task.ContinueWith(fun (t:Task<'T>) -> selector(t.Result))

    // TODO add missing
    static member SelectMany(input:Task<'T>, binder:'T -> Task<'I>, projection:'T -> 'I -> 'R): Task<'R> =
        let tcs = new TaskCompletionSource<'R>()    
        
        // missing code

        tcs.Task

    static member SelectMany(input:Task<'T>, binder:'T -> Task<'R>): Task<'R> =
        let tcs = new TaskCompletionSource<'R>()    
        
        // missing code

        tcs.Task
