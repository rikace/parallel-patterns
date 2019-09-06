namespace HelpersFSharp

module ImageHandler =

    open System
    open System.Drawing
    open System.IO
    open SixLabors.ImageSharp
    open SixLabors.ImageSharp.Formats.Jpeg
    open SixLabors.ImageSharp.PixelFormats
    open SixLabors.ImageSharp.Processing
    open SixLabors.ImageSharp.Processing.Processors
    open SixLabors.ImageSharp.Processing.Processors.Overlays
    open SixLabors.Primitives

    let combine (target:string, source1:string, source2:string) =
        let img1 = Image.Load(source1)
        let img2 = Image.Load(source2)
        let combo = img1.Clone()
        let pixelize (img:Image<Rgba32>) = [
            for x in 0..img.Width - 1 do
                for y in 0..img.Height - 1 do
                    yield (x,y, img.[x,y]) ]
        let pix1 = pixelize img1
        let pix2 = pixelize img2
        let sorter (_,_,c:Rgba32) = [c.R;c.G;c.B] |> Seq.max
        (pix1 |> List.sortBy sorter, pix2 |> List.sortBy sorter)
        ||> List.zip
        |> List.iter(fun ((x1,y1,_), (_,_,c2)) -> 
            combo.[x1,y1] <- c2)
        combo.Save(target)

