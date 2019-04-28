namespace ParallelPatterns.Fsharp

[<AutoOpen>]
module Interfaces = 

    open System
    open System.Threading.Tasks
        
    [<Interface>]
    type IAgent<'T, 'U> =
        abstract Send : 'T -> Task 
        abstract Post : 'T -> unit 
        abstract AsObservable : unit ->  IObservable<'U>
    
    [<Interface>]
    type IReplyAgent<'T, 'TReply> =
        abstract Send : 'T -> Task 
        abstract Post : 'T -> unit
        abstract Ask : 'T -> Task<'TReply>