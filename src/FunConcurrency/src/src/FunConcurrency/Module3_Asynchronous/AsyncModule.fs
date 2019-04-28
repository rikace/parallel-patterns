module FunConcurrency.Asynchronous.AsyncModule


#if INTERACTIVE
#load "../Common/Helpers.fs"
#load "../Module3_Asynchronous/Async.fs"
#r "System.Drawing.dll"
#endif

open System
open System.Threading
open System.Net
open System.IO
open FunConcurrency.Async.AsyncOperators
open FunConcurrency
open FunConcurrency.AsyncCombinators

type T = { Url : string }

let xs = [
    { Url = "http://microsoft.com" }
    { Url = "thisDoesNotExists" } // throws when constructing Uri, before downloading
    { Url = "https://thisDotNotExist.Either" }
    { Url = "http://google.com" }
]

let isAllowedInFileName c =
    not <| Seq.contains c (Path.GetInvalidFileNameChars())

let downloadAsync url =
    async {
        use client = new WebClient()
        printfn "Downloading %s ..." url
        let! data = client.DownloadStringTaskAsync(Uri(url)) |> Async.AwaitTask
        return (url, data)
    }

let saveAsync (url : string, data : string) =
    let destination = url // fix here to change the file name generation as needed
    async {
        let fn =
            [|
                __SOURCE_DIRECTORY__
                destination |> Seq.filter isAllowedInFileName |> String.Concat
            |]
            |> Path.Combine
        printfn "saving %s ..." (Path.GetFileName destination)
        use stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 0x100)
        use writer = new StreamWriter(stream)
        do! writer.WriteAsync(data) |> Async.AwaitTask
        return (url, data)
        }

xs
|> Seq.map (fun u -> downloadAsync u.Url)
|> Async.Parallel
|> Async.RunSynchronously
|> Seq.iter(fun (u: string,d: string) -> printfn "Downloaded %s - size %d" u d.Length)



// Step (1)
// How can you handle the exception in case of not existing URL?
// TIP:    avoid the try-catch block and check if there is an existing
//         "Async" api that could help you
//         In addition, you should still be able to branch logic in both the
//         "success" and "failure" paths using the F# Result<_,_> type
//
// FIX THE CODE BELOW REMOVING THE "try-with" block with a more idiomatic approach
// Tips: the build in "Result<_,_>" and "Choice*" types could be helpful. In this example,
// use the "AsyncRes" type define below as building block

// Extract the functionality into a "wrap" function

type AsyncRes<'a> = Async<Result<'a, exn>>


xs
|> List.map(fun u -> try
                        downloadAsync u.Url
                     with
                     | ex -> reraise())
|> Async.Parallel
|> Async.RunSynchronously
//|> Seq.iter (function
//    | Ok data -> printfn "Succeeded"
//    | Error exn -> printfn "Failed with %s" exn.Message)
|> ignore



// Step (2)
// How can you compose the "saveAsync" function at the end of the pipeline?
// Example: downloadAsync >> "hanlde errors" >> saveFunction
// TIP:  first attempt, a "map" function over async types could be useful, but it has to be
//       adapted to the "type AsyncRes" type

// TIP : define and use a new type that combines the Async and Result types,
//       this type definition will be very useful for the composition of the coming functions

xs
|> List.map(fun u -> (* missing code, remove the "async.Return u" with the correct implementation *)
                async.Return u) // THIS IS CONCEPTUAL CODE wrap (downloadAsync u.Url) |> map saveAsync)
|> Async.Parallel
|> Async.RunSynchronously
//|> Seq.iter (function
//    | Ok data -> printfn "Succeeded"
//    | Error exn -> printfn "Failed with %s" exn.Message)
|> ignore



// Step (3)
// How can you compose the "downloadAsync" and "saveAsync" functions
// in a more idiomatic way?
// Example: downloadAsync >> saveFunction
// TIP: start wrapping (or lifting) both the "downloadAsync" and "saveAsync" functions into a "type AsyncRes<_>" type
//      try to implement the map / bind/ flatMap function for the previously define type

let downloadAsync' url : AsyncRes<string * string> =
    // wrap the function "downloadAsync"
    Unchecked.defaultof<_>

let saveAsync' (url : string, data : string) : AsyncRes<string * string> =
    let destination = url // fix here to change the file name generation as needed
    // wrap the function "downloadAsync"
    Unchecked.defaultof<_>




// BONUS :
// try to use the "kleisli" combinator, and then create the "fish" infix operator >=> to replace the "kleisli" function
// kleisli signature: (f:'a -> AsyncRes<'b>) (g:'b -> AsyncRes<'c>) (x:'a)

let (>=>) (operation1:'a -> AsyncRes<'b>) (operation2:'b -> AsyncRes<'c>) (value:'a) : 'a -> AsyncRes<'c> = Unchecked.defaultof<_>

// let downloadAndSave = downloadAsync' >=> saveAsync'



// Step (4)
// now that we have implemented the Bind function, let's implement a computation expression
// https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/computation-expressions
//
// complete the Bind functions for the different siganutures.
// in this example, it is important to define two Bind functions, as indicated below
// NOTE: you should be able to use any implementaion of the "download" & "save" function
//       (the wrapped and no wrapped version)

type AsyncResComputationExpression() =

    member this.Bind(x: AsyncRes<'a>, f: 'a -> AsyncRes<'b>) : AsyncRes<'b> = Unchecked.defaultof<_>

    member this.Bind (m:Async<'a>, f:'a -> AsyncRes<'b>) : AsyncRes<'b> = Unchecked.defaultof<_>

    member this.Bind(result : Result<'a, exn>, binder : 'a -> AsyncRes<'b>) : AsyncRes<'b> = Unchecked.defaultof<_>

    member this.Delay(f) = f()
    member this.Return m = AsyncRes.retn m
    member this.ReturnFrom (m : AsyncRes<'a>) = m
    member this.Zero() : AsyncRes<unit> = this.Return()


// Testing
let asyncRes = AsyncResComputationExpression()

let comp url = asyncRes {
    let! resDownload = downloadAsync url
    let! resSaving = saveAsync resDownload
    return resSaving
    }

xs
|> List.map(fun u -> comp u.Url)
|> Async.Parallel
|> Async.RunSynchronously
|> Seq.iter (function
    | Ok data -> printfn "Succeeded"
    | Error exn -> printfn "Failed with %s" exn.Message)
