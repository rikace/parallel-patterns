// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
    
    AsyncModule.runAsync()
    |> Seq.iter (fun s -> printfn "%s" s)

    AsyncModule.runAsyncThrottle()
    |> Seq.iter (fun s -> printfn "%s" s)
    
    0 // return an integer exit code
