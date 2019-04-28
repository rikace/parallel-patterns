open System

open FunConcurrency.AsyncCombinators

[<EntryPoint>]
let main argv =


    let agent = new FunConcurrency.AgentWebCrawler.ParallelWebCrawler.WebCrawler()

    agent.Submit "https://www.google.com"

    Console.ReadLine() |> ignore

    agent.Dispose()

    0

