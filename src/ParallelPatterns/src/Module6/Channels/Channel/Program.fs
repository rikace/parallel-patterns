module Program

open System.Collections.Generic
open System.Collections.Concurrent
open System.Threading.Tasks
open System.Linq
open System.IO
open System
open System.Text.RegularExpressions
open System.Drawing
open System.Threading
open Channel
open SixLabors.ImageSharp
open SixLabors.ImageSharp.PixelFormats

type ImageInfo = { Path:string; Name:string; Image:Image<Rgba32>}

[<EntryPoint>]
let main argv =

    let convertImageTo3D (image:Image<Rgba32>) =
       Helpers.ImageHandler.ConvertTo3D(image)

    let chanLoadImage = ChannelAgent<string>()
    let chanApply3DEffect = ChannelAgent<ImageInfo>()
    let chanSaveImage = ChannelAgent<ImageInfo>()

    subscribe chanLoadImage (fun image ->
        let bitmap = Image.Load(image)
        let imageInfo = { Path = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
                          Name = Path.GetFileName(image)
                          Image = bitmap }
        chanApply3DEffect.Send imageInfo |> run)

    subscribe chanApply3DEffect (fun imageInfo ->
        let bitmap = convertImageTo3D imageInfo.Image
        let imageInfo = { imageInfo with Image = bitmap }
        chanSaveImage.Send imageInfo |> run)

    subscribe chanSaveImage (fun imageInfo ->
        printfn "Saving image %s" imageInfo.Name
        let destination = Path.Combine(imageInfo.Path, imageInfo.Name)
        imageInfo.Image.Save(destination))

    let loadImages() =
        let images = Directory.GetFiles("../../../../../Data/paintings")

        for image in images do
            chanLoadImage.Send image |> run

    loadImages()

    Console.ReadLine() |> ignore
    0
