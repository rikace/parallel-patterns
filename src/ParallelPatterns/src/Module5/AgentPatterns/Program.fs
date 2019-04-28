open System
open System.IO
open Helpers
open WebCrawler
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open AgentPatterns.ParallelWorker
open AgentPatterns
open System.Threading
open ImageHandlers

[<EntryPoint>]
let main argv =
    
    // TODO : 
    //      ParallelAgentPipeline
    //      implement a reusable parallel worker model built on F# agents 
    //      open the file "ParallelWorker.fs" and complete the TODOs
    //      to complete the function "parallelWorker"
    ParallelAgentPipeline.start()


    // TODO :
    //      ImagePipeline
    //      Implement a baunded-pipeline using the Agent as asynchronous buffer
    //      check the file "AsyncBoundedQueue.fs" for the underlying implementation
    //      open the file "ImagePipeline.fs" and complete the TODOs
    ImagePipeline.start()

    // TODO :
    //      AgentWebCrawler
    //      open the file "CrawlerAgent.fs" and complete the TODOs
    AgentWebCrawler.start()
    
    Console.ReadLine() |> ignore
    0 