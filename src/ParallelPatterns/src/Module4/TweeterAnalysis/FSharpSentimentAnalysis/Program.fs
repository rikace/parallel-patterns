
open System
open Tweetinvi.Models;
open System.Reactive.Linq
open System.Reactive.Concurrency
open System.Reactive
open System.Threading.Tasks

open FSharpSentimentAnalysis.SentimentAnalysis

[<EntryPoint>]
let main argv =
    
    let colorEmotion emotion =
        match emotion with
        | Unhappy -> System.ConsoleColor.Red
        | Indifferent -> System.ConsoleColor.Cyan
        | Happy -> System.ConsoleColor.Green
        
    let print (text : string) color =
        let bakColor = Console.ForegroundColor
        Console.ForegroundColor <- color        
        Console.WriteLine(text)
        Console.ForegroundColor <- bakColor;

    let obs =  DataProducer.TweetStream.GetReactiveTweets(TimeSpan.FromMilliseconds(150.))
                    .SubscribeOn(TaskPoolScheduler.Default)
               |> Observable.map(fun m -> scoreToSentiment m.Text, m)
                        
    let disposable = obs.Subscribe(fun (m, t) ->
        colorEmotion m |> print (sprintf "%O - %s" m t.Text))
    
    Console.WriteLine("Press `Enter` to exit.")
    Console.WriteLine("======================")
    Console.ReadLine() |> ignore
    0
