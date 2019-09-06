module ImagePipeline

open Helpers
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open System.Threading
open System.IO
open Microsoft.FSharp.Control
open System.Collections.Generic

module AsyncQueue =

    // represent a queue operation
    type Instruction<'T> =
        | Enqueue of 'T * (unit -> unit) 
        | Dequeue of ('T -> unit)

    type AsyncBoundedQueue<'T> (capacity: int, ?cancellationToken:CancellationTokenSource) =
        let waitingConsumers, elts, waitingProducers = Queue(), Queue<'T>(), Queue()
        let cancellationToken = defaultArg cancellationToken (new CancellationTokenSource())

(*  The following balance function shuffles as many elements through the queue 
    as possible by dequeuing if there are elements queued and consumers waiting 
    for them and enqueuing if there is capacity spare and producers waiting *)
        let rec balance() =
            if elts.Count > 0 && waitingConsumers.Count > 0 then
                elts.Dequeue() |> waitingConsumers.Dequeue()
                balance()
            elif elts.Count < capacity && waitingProducers.Count > 0 then
                let x, reply = waitingProducers.Dequeue()
                reply()
                elts.Enqueue x
                balance()

(*  This agent sits in an infinite loop waiting to receive enqueue and dequeue instructions, 
    each of which are queued internally before the internal queues are rebalanced *)
        let agent = MailboxProcessor.Start((fun inbox ->
                let rec loop() = async { 
                        let! msg = inbox.Receive()
                        match msg with
                        | Enqueue(x, reply) -> waitingProducers.Enqueue (x, reply)
                        | Dequeue reply -> waitingConsumers.Enqueue reply
                        balance()
                        return! loop() }
                loop()), cancellationToken.Token)

        member __.AsyncEnqueue x =
              agent.PostAndAsyncReply (fun reply -> Enqueue(x, reply.Reply))
        member __.AsyncDequeue() =
              agent.PostAndAsyncReply (fun reply -> Dequeue reply.Reply)

        interface System.IDisposable with          
              member __.Dispose() = 
                cancellationToken.Cancel()
                (agent :> System.IDisposable).Dispose()
                
type ImageInfo = { Name:string; Destination:string; mutable Image:Image<Rgba32>}

let capacityBoundedQueue = 10
let cts = new CancellationTokenSource()

let loadedImages = new AsyncQueue.AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)
let scaledImages = new AsyncQueue.AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)    
let filteredImages = new AsyncQueue.AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)    


// string -> string -> ImageInfo
let loadImage imageName destination = async {
    printfn "%s" (imageName)
    let mutable image = Image.Load(imageName)    
    return { Name = imageName; 
             Destination = Path.Combine(destination, Path.GetFileName(imageName)); 
             Image=image }
    }

// ImageInfo -> ImageInfo  
let scaleImage (info:ImageInfo) = async {
    let scale = 200    
    let image = info.Image.Clone()
    let image' = info.Image   
    info.Image.Dispose()
    info.Image <- null 
    let mutable resizedImage = ImageHandlers.resize scale scale image 
    return { info with Image = resizedImage } }

let filterRed (info:ImageInfo) filter = async {
    let image = info.Image.Clone()     
    let mutable filterImage = ImageHandlers.setFilter filter image
    return { info with Image = filterImage } }
    
let loadImages = async {
    let images = Directory.GetFiles("../../../../../Data/paintings", "*.jpg")
    let destinationFolder = "../../../../../Data/Images/Output"
    if Directory.Exists destinationFolder |> not then
        Directory.CreateDirectory destinationFolder |> ignore
        
    for image in images do 
        printfn "Loading %s" image 
        let! info = loadImage image "../../../../../Data/Images/Output"
        do! loadedImages.AsyncEnqueue(info) }

let scaleImages = async {
    while not cts.IsCancellationRequested do 
        let! info = loadedImages.AsyncDequeue()
        let! info = scaleImage info
        do! scaledImages.AsyncEnqueue(info) }

let filters = [| ImageFilters.Blue; ImageFilters.Green;
                 ImageFilters.Red; ImageFilters.Gray |]

let filterImages = async {
    while not cts.IsCancellationRequested do         
        let! info = scaledImages.AsyncDequeue()
        for filter in filters do
            let! imageFiltered = filterRed info filter
            do! filteredImages.AsyncEnqueue(imageFiltered) }
 
 // TODO :
 //     complete the saveImages async function
 //     look how the previous functions "fileterImagesd" and "scaleImages" are implemented
let saveImages = async {
   
       return ()  }

 // TODO :
 //     complete the "start" function
 //     run all the steps previously implemented 
 //     loadImages, scaleImages, filterImages, saveImages

let start() = ()

                