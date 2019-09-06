module FSharpSentimentAnalysis.SentimentAnalysis

open System
open System.Collections.Generic
open System.Reactive.Linq
open Tweetinvi.Models
open Tweetinvi
open Tweetinvi.Streaming.Parameters
open Analysis
open System.Reactive.Linq
open System.Reactive.Concurrency

type Emotion =
    | Unhappy
    | Indifferent
    | Happy

let getEmotionMeaning value =
    match value with
    | 0 | 1 -> Unhappy
    | 2 -> Indifferent
    | _ (* 3 | 4 *) -> Happy

type EmotionType =
     { emotion:int }
       with
         static member zero = { emotion = 0 }
         
         static member consoleColor (x:EmotionType) =
            match x.emotion with
            | 0 | 1 -> System.ConsoleColor.Red
            | 2 -> System.ConsoleColor.Cyan
            | 3 | 4 -> System.ConsoleColor.Green
            | x -> System.ConsoleColor.Yellow
             
         member x.toColor() =
            match x.emotion with
            | 0 | 1 -> "red"
            | 2 -> "green"
            | 3 -> "blue"
            | 4 -> "yellow"
            | x -> failwith (sprintf "Unknown emotion value %d" x)
            
         override this.ToString() =
            match this.emotion with
            | 0 | 1 -> "Unhappy"
            | 2 -> "Indifferent"
            | 3 | 4 -> "Happy"
            | x -> failwith (sprintf "Unknown emotion value %d" x)
    
let either a b = if String.IsNullOrEmpty a then b else a

let normalize (em:float32) =
     // Happy
     //Val : 4 - max : 406.204900  - min : -189.103600
     //Val : 3 - max : 431.193100  - min : -331.100800            
     // INdifferent 
     // Val : 2 - max : 419.557500  - min : -311.066700            
     // Unhappy
     // Val : 1 - max : 430.839700  - min : -451.206700
     // Val : 0 - max : 238.980100  - min : -141.332000
     if em <= 406.204900f && em >= 306.103600f then 4
     elif em <= 306.103600f && em >= 0.f then 3
     elif em <= 0.f && em >= -221.206700f then 2
     elif em <= -221.206700f && em >= -431.100800f then 1
     else 0

        
let runPrediction =
     let sentimentModel = ML.loadModel "../../../Data/SentimentAnalysis/model.zip"
     ML.runPrediction sentimentModel
     
let score = runPrediction >> ML.scorePrediction >> normalize
let scoreToSentiment = score >> getEmotionMeaning

// Settings to enable the Twitterinvi library
// Create new Twitter application and copy-paste
// keys and access tokens to module variables
let consumerKey = "<your Key>"
let consumerSecretKey = "<your secret key>"
let accessToken = "<your access token>"
let accessTokenSecret = "<your secreat access token>"

let cred = new TwitterCredentials(consumerKey, consumerSecretKey, accessToken, accessTokenSecret)
let stream = Stream.CreateSampleStream(cred)
stream.FilterLevel <- StreamFilterLevel.Low

let emotionMap =
    [(Unhappy, 0)
     (Indifferent, 0)
     (Happy, 0)] |> Map.ofSeq

// Observable pipeline to analyze the tweets
let observableTweets =
    stream.TweetReceived  
    |> Observable.throttle(TimeSpan.FromMilliseconds(50.))  
    |> Observable.filter(fun args ->
        args.Tweet.Language = Language.English) 
    |> Observable.groupBy(fun args ->
        scoreToSentiment args.Tweet.FullText) 
    |> Observable.selectMany(fun args ->
        args |> Observable.map(fun i ->
            (args.Key, (max 1 i.Tweet.FavoriteCount))))  
    |> Observable.scan(fun sm (key,count) ->
        match sm |> Map.tryFind key with
        | Some(v) -> sm |> Map.add key (v + count)
        | None    -> sm ) emotionMap  
    |> Observable.map(fun sm ->
        let total = sm |> Seq.sumBy(fun v -> v.Value)  
        sm |> Seq.map(fun k ->
            let percentageEmotion = ((float k.Value) * 100.) / (float total)
            let labelText = sprintf "%A - %.2f.%%" (k.Key) percentageEmotion
            (labelText, percentageEmotion)
        ))


// Struct TweetEmotino to maintain the tweet details
[<Struct>]
type TweetEmotion(tweet:ITweet, emotion:Emotion) =
    member this.Tweet with get() = tweet
    member this.Emotion with get() = emotion

    static member Create tweet emotion =
        TweetEmotion(tweet, emotion)

// Implementation of Observable Tweet-Emotions
let tweetEmotionObservableOnline(throttle:TimeSpan) =
    Observable.Create(fun (observer:IObserver<_>) ->  
        let cred = new TwitterCredentials(consumerKey, consumerSecretKey, accessToken, accessTokenSecret)
        let stream = Stream.CreateSampleStream(cred)
        stream.FilterLevel <- StreamFilterLevel.Low
        stream.StartStreamAsync() |> ignore

        stream.TweetReceived
        |> Observable.throttle(throttle)
        |> Observable.filter(fun args ->
            args.Tweet.Language = Language.English)
        |> Observable.groupBy(fun args ->
            scoreToSentiment args.Tweet.FullText)
        |> Observable.selectMany(fun args ->
            args |> Observable.map(fun tw -> TweetEmotion.Create tw.Tweet args.Key))
        |> Observable.subscribe(observer.OnNext)  
    )

let tweetEmotionObservableOffline (throttle:TimeSpan) =
    Observable.Create(fun (observer:IObserver<_>) ->        
        let tweetStream = DataProducer.TweetStream.GetReactiveTweets(throttle)
        
        tweetStream        
//        |> Observable.filter(fun args ->
//            args.Language = Language.English)
        |> Observable.map(fun args ->
            let sentiment = scoreToSentiment args.FullText
            TweetEmotion.Create args sentiment)
//        |> Observable.selectMany(fun args ->
//            args |> Observable.map(fun tw ->
//                TweetEmotion.Create tw args.Key))
        |> Observable.subscribe(observer.OnNext)  
    )
    
let printUnhappyTweets() =
    { new IObserver<TweetEmotion> with
        member this.OnNext(tweet) = ()
        member this.OnCompleted() = ()
        member this.OnError(exn) = ()
    }
