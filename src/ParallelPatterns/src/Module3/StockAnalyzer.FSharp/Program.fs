// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
   
    StockAnalyzer.analyzeStockHistory()

    Console.ReadLine() |> ignore
    0 // return an integer exit code
