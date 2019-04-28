module ImageHandlers

open SixLabors.ImageSharp
open HelpersFSharp
open Helpers
open SixLabors.ImageSharp.PixelFormats
open System.IO


let load (imageSrc:string) = Image.Load(imageSrc)

let resize width height image = ImageHandler.Resize(image, width, height)

let convert3D image = ImageHandler.ConvertTo3D image

let setFilter filter image = ImageHandler.SetFilter(image, filter)

let saveImage (destination:string) (image:Image<Rgba32>) =
    use stream = File.Create(destination)
    image.SaveAsJpeg(stream)
    
module Interpolation =
        
    let lerp (s:float32) (e:float32) (t:float32) =
        s + (e - s) * t
     
    let blerp c00 c10 c01 c11 tx ty =
        lerp (lerp c00 c10 tx) (lerp c01 c11 tx) ty
    
    // EXAMPLE : scale image 1.6 1.6
    // TODO : parallize 
    let scale (self:Image<Rgba32>) (scaleX:float) (scaleY:float) =
        let newWidth  = int ((float self.Width)  * scaleX)
        let newHeight = int ((float self.Height) * scaleY)    
        let newImage = new Image<Rgba32>(newWidth, newHeight)
        for x in 0..newWidth-1 do
            for y in 0..newHeight-1 do
                let gx = (float32 x) / (float32 newWidth) *  (float32 (self.Width  - 1))
                let gy = (float32 y) / (float32 newHeight) * (float32 (self.Height - 1))
                let gxi = int gx
                let gyi = int gy
                let c00 = self.[gxi, gyi]
                let c10 = self.[gxi + 1, gyi]
                let c01 = self.[gxi,     gyi + 1]
                let c11 = self.[gxi + 1, gyi + 1]
                let red   = (blerp (float32 c00.R) (float32 c10.R) (float32 c01.R) (float32 c11.R) (gx - (float32 gxi)) (gy - (float32 gyi)))
                let green = (blerp (float32 c00.G) (float32 c10.G) (float32 c01.G) (float32 c11.G) (gx - (float32 gxi)) (gy - (float32 gyi)))
                let blue  = (blerp (float32 c00.B) (float32 c10.B) (float32 c01.B) (float32 c11.B) (gx - (float32 gxi)) (gy - (float32 gyi)))
                newImage.[x, y] <- Rgba32(red, green,  blue)
        newImage
