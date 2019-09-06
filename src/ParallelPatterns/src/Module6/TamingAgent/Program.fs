module Program

open System.IO
open System
open System.Drawing
open TamingAgentModule
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

[<AutoOpen>]
module HelperType =
    type ImageInfo = { Path:string; Name:string; Image:Image<Rgba32>}

module ImageHelpers =
 

    let convertImageTo3D (image:Image<Rgba32> ) =
        Helpers.ImageHandler.ConvertTo3D(image)

    // The TamingAgent in action for image transformation
    let loadImage = (fun (imagePath:string) -> async {
        let bitmap = Image.Load(imagePath)
        return { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                 Name = Path.GetFileName(imagePath)
                 Image = bitmap } })

    let apply3D = (fun (imageInfo:ImageInfo) -> async {
        let bitmap = convertImageTo3D imageInfo.Image
        return { imageInfo with Image = bitmap } })

    let saveImage = (fun (imageInfo:ImageInfo) -> async {
        printfn "Saving image %s" imageInfo.Name
        let destination = Path.Combine(imageInfo.Path, imageInfo.Name)
        imageInfo.Image.Save(destination)
        return imageInfo.Name})


module ``TamingAgent example`` =

    open AsyncEx
    open ImageHelpers

    let loadandApply3dImage imagePath = Async.retn imagePath >>= loadImage >>= apply3D >>= saveImage

    let loadandApply3dImageAgent = TamingAgent<string, string>(2, loadandApply3dImage)

    let _ = loadandApply3dImageAgent.Subscribe(fun imageName -> printfn "Saved image %s - from subscriber" imageName)

    let transformImages() =
        let images = Directory.GetFiles(@".\Images", "*.jpg")
        for image in images do
            loadandApply3dImageAgent.Ask(image) |> run (fun imageName -> printfn "Saved image %s - from reply back" imageName)


module ``Composing TamingAgent with Kleisli operator example`` =
    open Kleisli
    open AsyncEx
    open ImageHelpers

    // The TamingAgent with Kleisli operator
    let pipe (limit:int) (operation:'a -> Async<'b>) (job:'a) : Async<_> =
        let agent = TamingAgent(limit, operation)
        agent.Ask(job)

    let loadImageAgent = pipe 2 loadImage
    let apply3DEffectAgent = pipe 2 apply3D
    let saveImageAgent = pipe 2 saveImage
  
    // >=>
    let pipeline = loadImageAgent >=> apply3DEffectAgent >=> saveImageAgent

    let transformImages() =
        let images = Directory.GetFiles(@".\Images", "*.jpg")
        for image in images do
            pipeline image |> run (fun imageName -> printfn "Saved image %s" imageName)



[<EntryPoint>]
let main argv =

    ``TamingAgent example``.transformImages()

    ``Composing TamingAgent with Kleisli operator example``.transformImages();

    Console.ReadLine() |> ignore
    0