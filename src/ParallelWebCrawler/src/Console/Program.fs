open System

open FSharpWebCrawler.AsyncCombinators

[<EntryPoint>]
let main argv =


    let agent = new FSharpWebCrawler.AgentWebCrawler.ParallelWebCrawler.WebCrawler()

    agent.Submit "https://www.google.com"

    Console.ReadLine() |> ignore

    agent.Dispose()

    0



