module FSharpWebCrawler.MessagePassing.AgentPipeline

#if INTERACTIVE
#load "../Common/Helpers.fs"
#load "../Module3_Asynchronous/Async.fs"
#r "System.Drawing.dll"
#endif

open System
open System.Threading
open System.Net
open System.IO
open System.Drawing
open FSharpWebCrawler.Async.AsyncOperators
open FSharpWebCrawler
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

// Step (1) implement a structured agent that returns
//          the result of the "computation" over a message received
//
//          try to handle messages that could run a computation either "sync" or "async"
//          TIP: you could have a DU to handle a different type of message (for either a Sync or Async computation)
//               or to load the computation at runtime. In this last case, the Agent body should keep an
//               internal state of the function passed

let agent computation = Agent<'a * AsyncReplyChannel<'b>>.Start(fun inbox ->
    let rec loop () = async {

        // MISSING CODE

        return! loop() }
    loop() )


// Testing
let myAgent = agent (fun (x : int) -> sprintf "the square of %d is %d" x (x * x))
let res = myAgent.PostAndReply(fun ch -> (6, ch))
printfn "%s" res


// Step (2) implement the "pipeline" function to compose agents, where the result of the
//          first "Agent" is passed to the second "Agent"
//          The idead of this function is to use the previously implemented
//          well structured agent (in step 1), to pass a message, process the message,
//          and return the result of the agent compoutation to the next "Agent"
//          BONUS - Try also to implement a function that handle Async computation

let pipelineAgent (f:'a -> 'b) (value : 'a) : Async<'b> =
    // MISSING CODE
    Unchecked.defaultof<_> // << remove this line


// Step (3) compose pipeline
// given two agents (below), compose them into a pipeline
// in a way that calling (or sending a message) to the pipeline,
// the message is passed across all the agents in the pipelin

// TIP: Remember the "async bind" operator?
//      the signature of the Async.bind operator fits quite well in this context,
//      because the return type of the "pipelineAgent" function is an "Async<_>"
// TIP: It could be useful to use an infix operator to simplify the composition between Agents
// BONUS: after have composed the agents, try to use (and implement) the Kliesli operator

// this function can be found in the module AsyncEx
// (‘a -> Async<’b>) -> Async<’a> -> Async<’b>
let agentBind f xAsync = async {
    let! x = xAsync
    return! f x }

let agentRetn x = async { return x }



// Testing
let agent1 = pipelineAgent (sprintf "Pipeline 1 processing message : %s")
let agent2 = pipelineAgent (sprintf "Pipeline 2 processing message : %s")

let message i = sprintf "Message %d sent to the pipeline" i

for i in [0..5] do
    agent1 (string i) |> Async.run (fun res -> printfn "%s" res)


let pipeline x = agentRetn x >>= agent1 >>= agent2

for i in [1..10] do
    pipeline (string i)
    |> Async.run (fun res -> printfn "Thread #id: %d - Msg: %s" Thread.CurrentThread.ManagedThreadId res)


let pipeline' = agent1 >=> agent2

let operation i = pipeline' <| message i



// Step (4) Each agent in the pipeline handles one message at a give time
//    How can you make these agents running in parallel?
//    This is important in the case of async computations, so you can reach great throughput
//
//     create a "parallelAgent" worker based on the MailboxProcessor.
//     the idea is to have an Agent that handles, computes and distributes the messages
//     in a Round-Robin fashion between a set of (intern and pre-instantiated) Agent children

let cts = new CancellationTokenSource()

let parallelAgent (parallelism: int) (computation: 'a -> Async<'b>) =
    let token = cts.Token

    let behavior = (fun (inbox: MailboxProcessor<'a * AsyncReplyChannel<'b>>) ->
        let rec loop () = async {
            let! msg, replyChannel = inbox.Receive()
            let! res = computation msg
            replyChannel.Reply res
            return! loop() }
        loop() )

    // MISSING CODE HERE
    // 1 - use the "Array" module to initalize an array of Agents.
    //     the "behavior" agent is useful to handle the reply-channel
    let agents = Unchecked.defaultof<MailboxProcessor<_> []> // << replace this line with implementation

    // 2 - crete an agent that broadcasts the messages received
    //     in a Round-Robin fashion between the agents created in the  previous point
    let agent = new Agent<_>((fun inbox ->
        let rec loop index = async {
            let! msg = inbox.Receive()
            // MISSING CODE HERE

            return! loop index
        }
        loop 0), cancellationToken = token)

    token.Register(fun () -> agents |> Seq.iter(fun agent -> (agent :> IDisposable).Dispose())) |> ignore
    agent.Start()
    fun (a: 'a) -> agent.PostAndAsyncReply(fun ch -> (a, ch)) : Async<'b>


module AgentComposition =

    open System.IO
    open System
    open System.Drawing

    [<AutoOpen>]
    module HelperType =
        type ImageInfo = { Path:string; Name:string; Image:Image<Rgba32> }

    module ImageHelpers =
        let convertImageTo3D (image:Image<Rgba32>) =
            let bitmap = image.Clone()
            let w,h = bitmap.Width, bitmap.Height
            for x in 20 .. (w-1) do
                for y in 0 .. (h-1) do
                    let c1 = bitmap.[x,y]
                    let c2 = bitmap.[x - 20,y]
                    let color3D = Rgba32(c1.R, c2.G, c2.B)
                    bitmap.[x - 20 ,y] <- color3D
            bitmap

        let loadImage = (fun (imagePath:string) -> async {
            printfn "loading image %s" (Path.GetFileName(imagePath))
            let bitmap = Image.Load(imagePath)
            return { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                     Name = Path.GetFileName(imagePath)
                     Image = bitmap } })

        let apply3D = (fun (imageInfo:ImageInfo) -> async {
            printfn "destination image %s >> %s" imageInfo.Name imageInfo.Path
            let bitmap = convertImageTo3D imageInfo.Image
            return { imageInfo with Image = bitmap } })

        let saveImage = (fun (imageInfo:ImageInfo) -> async {
            printfn "Saving image %s" imageInfo.Name
            let destination = Path.Combine(imageInfo.Path, imageInfo.Name)
            imageInfo.Image.Save(destination)
            return imageInfo.Name})

    open ImageHelpers


    // Step (6) apply the parallelAgent to run the below function "loadAndApply3dImageAgent"
    let loadAndApply3dImage imagePath = agentRetn imagePath >>= loadImage >>= apply3D >>= saveImage
    let loadAndApply3dImageAgent = parallelAgent 2 loadAndApply3dImage

    // Step (7) use the "pipeline" function created in step (2), and replace the basic "agent"
    //          with the "parallelAgent". keep in mind of the extra parameter "limit" to indicate
    //          the level of parallelism
    //          - the "Unchecked.defaultof<_>" is a place-holder, replace it with the implementation

    let loadImageAgent : string -> Async<ImageInfo> = Unchecked.defaultof<_>
    let apply3DEffectAgent : ImageInfo -> Async<ImageInfo> =  Unchecked.defaultof<_>
    let saveImageAgent : ImageInfo -> Async<string> =  Unchecked.defaultof<_>

    // Step (8) compose the previuous function
    //          - loadImageAgent, apply3DEffectAgent, saveImageAgent
    let parallelPipeline :string -> Async<string> = Unchecked.defaultof<_>

    let parallelTransformImages() =
       let images = Directory.GetFiles(Environment.CurrentDirectory + @"/src/FunConcurrency/Images")
       for image in images do
            parallelPipeline image |> Async.run (fun imageName -> printfn "Saved image %s" imageName)

    // Testing
    parallelTransformImages()
