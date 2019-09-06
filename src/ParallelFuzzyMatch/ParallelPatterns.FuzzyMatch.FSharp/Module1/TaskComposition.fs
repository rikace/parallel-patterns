namespace ParallelPatterns.Fsharp
open System
open System.Threading.Tasks
open System.Runtime.CompilerServices

[<Extension>]
module TaskCompositionEx =
    [<Extension>]
    type TaskEx =
        [<Extension>]
        static member Then (input : Task<'T>, 
                            binder :Func<'T, Task<'U>>) =
            let tcs = new TaskCompletionSource<'U>()
            
            // missing code

            tcs.Task
            
        // TODO (1)
        // implement missing code
        [<Extension>]
        static member  Then (input : Task<'T>, 
                             binder :Func<'T, Task<'U>>, 
                             projection:Func<'T, 'U, 'R>) =
            let tcs = new TaskCompletionSource<'U>()
            
            // missing code

            tcs.Task

        // TODO (1)
        // implement missing code
        [<Extension>]
        static member Then (input : Task<'T>, 
                            binder :Func<'T, 'U>) =
            let tcs = new TaskCompletionSource<'U>()
            
            // missing code

            tcs.Task


        [<Extension>]
        static member Select (task : Task<'T>, selector :Func<'T, 'U>) : Task<'U> =
            let tcs = new TaskCompletionSource<'U>()
            
            // missing code

            tcs.Task
