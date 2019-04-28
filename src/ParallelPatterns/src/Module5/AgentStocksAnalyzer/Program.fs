// Learn more about F# at http://fsharp.org

open System
open Agents
open Messages

[<EntryPoint>]
let main argv =

    let lineChartingAgent = lineChartingAgent()
    let stocksCoordinatorAgent = stocksCoordinatorAgent(lineChartingAgent)
    for stock, color in ["MSFT", ConsoleColor.Red; "APPL", ConsoleColor.Cyan; "GOOG", ConsoleColor.Green] do
      stocksCoordinatorAgent.Post(WatchStock(stock, color))


    Console.ReadLine() |> ignore
    0 