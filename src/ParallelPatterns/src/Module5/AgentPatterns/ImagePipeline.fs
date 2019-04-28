module ImagePipeline

open Helpers
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats
open System.Threading
open AgentPatterns
open System.IO

type ImageInfo = { Name:string; Destination:string; mutable Image:Image<Rgba32>}

let capacityBoundedQueue = 10
let cts = new CancellationTokenSource()

let loadedImages = new AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)
let scaledImages = new AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)    
let filteredImages = new AsyncBoundedQueue<ImageInfo>(capacityBoundedQueue, cts)    


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
    let images = Directory.GetFiles("../../Data/paintings")
    for image in images do 
        printfn "Loading %s" image 
        let! info = loadImage image "../../Data/Images"
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
    return ()   }

 // TODO :
 //     complete the "start" function
 //     run all the steps previously implemented 
 //     loadImages, scaleImages, filterImages, saveImages

let start() = ()


                