module FSharpWebCrawler.AsyncCombinators

///Type representing a future / promise with Result constr
type AsyncRes<'a> = Async<Result<'a, exn>>



///Functions for working with the AsyncRes<'a> type
[<RequireQualifiedAccess>]
module AsyncRes =

    // Convert a choice to an outcome
    let ofChoice = function
        | Choice1Of2 value -> Result.Ok value
        | Choice2Of2 e -> Result.Error e


    // Create a AsyncRes from an async computation
    let wrap (computation : Async<'a>) : AsyncRes<'a> = Unchecked.defaultof<_>

    // Create a AsyncRes from a value
    let retn x =
        wrap (async { return x })

    // Map the success of a future
    let flatMap (f: 'a -> AsyncRes<'b>) (future : AsyncRes<'a>) = Unchecked.defaultof<_>

    ///Bind two futures returning functions together
    let bind f g = f >> (flatMap g)

    let (>>=) f g = flatMap g f
