open AgentWebCrawler.ParallelAgentWebCrawler
open System

[<EntryPoint>]
let main argv =
    
    //let agent = new ParallelWebCrawler.WebCrawler(8)
    let agent = new SyncWebCrawler.WebCrawler(8)
    
    agent.Submit "https://www.google.com"
    
    Console.ReadLine() |> ignore
    
    agent.Dispose()
    
    0
