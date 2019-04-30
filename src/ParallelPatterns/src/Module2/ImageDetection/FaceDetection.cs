using System;

namespace ImageDetection
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CommonHelpers;
    using Emgu.CV;
    using Emgu.CV.Structure;
    using ParallelPatterns.TaskComposition;

    public class FaceDetection
    {
        public static void DetectFaces(string imageSource, string imageFolderDestination)
        {
            var imageFrame = new Image<Bgr, byte>(imageSource);
            var cascadeClassifier = new CascadeClassifier("haarcascade_frontalface_alt_tree.xml");
            var grayframe = imageFrame.Convert<Gray, byte>();

            var faces = cascadeClassifier.DetectMultiScale(
                grayframe, 1.1, 3, System.Drawing.Size.Empty);
            foreach (var face in faces)
                imageFrame.Draw(face, new Bgr(System.Drawing.Color.DarkRed), 3);

            var image = imageFrame.ToBitmap();

            image.Save(Path.Combine(imageFolderDestination, Path.GetFileName(imageSource)));
        }

        // Using "ThreadLocal" for thread safety
        static ThreadLocal<CascadeClassifier> CascadeClassifierThreadLocal =
            new ThreadLocal<CascadeClassifier>(() => new CascadeClassifier("haarcascade_frontalface_alt_tree.xml"));


        // This can be used thread-safely because the ThreadLocal
        public static void DetectFacesThreadLocal(string imageSource, string imageFolderDestination)
        {
            var imageFrame = new Image<Bgr, byte>(imageSource);
            var cascadeClassifier = CascadeClassifierThreadLocal.Value;
            var grayframe = imageFrame.Convert<Gray, byte>();

            var faces = cascadeClassifier.DetectMultiScale(
                grayframe, 1.1, 3, System.Drawing.Size.Empty);
            foreach (var face in faces)
                imageFrame.Draw(face, new Bgr(System.Drawing.Color.DarkRed), 3);

            var image = imageFrame.ToBitmap();

            image.Save(Path.Combine(imageFolderDestination, Path.GetFileName(imageSource)));
        }

        public static Task MultiFaceDetection(string imagesFolder, string imageFolderDestination = null)
        {
            var filePaths = Directory.GetFiles(imagesFolder);
            //var imageFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            imageFolderDestination = imageFolderDestination ?? @"c:\Temp";

            var bitmapTaskss = from filePath in filePaths
                select Task.Run(() => DetectFacesThreadLocal(filePath, imageFolderDestination)); // ToArray

            return Task.WhenAll(bitmapTaskss);
        }


        // DetectFaces function using Task-Continuation
        public static Task DetectFacesContinuation(string imageSource, string imageFolderDestination)
        {
            var imageTask = Task.Run<Image<Bgr, byte>>(
                () => new Image<Bgr, byte>(imageSource)
            );
            var imageFrameTask = imageTask.ContinueWith(
                image => image.Result.Convert<Gray, byte>()
            );
            var grayframeTask = imageFrameTask.ContinueWith(
                imageFrame => imageFrame.Result.Convert<Gray, byte>()
            );

            var facesTask = grayframeTask.ContinueWith(grayFrame =>
                {
                    var cascadeClassifier = CascadeClassifierThreadLocal.Value;
                    return cascadeClassifier.DetectMultiScale(
                        grayFrame.Result, 1.1, 3, System.Drawing.Size.Empty);
                }
            );

            var bitmapTask = facesTask.ContinueWith(faces =>
                {
                    foreach (var face in faces.Result)
                        imageTask.Result.Draw(
                            face, new Bgr(System.Drawing.Color.BurlyWood), 3);
                    return imageTask.Result.ToBitmap();
                }
            );

            return bitmapTask.ContinueWith(image =>
                image.Result.Save(Path.Combine(imageFolderDestination, Path.GetFileName(imageSource))));
        }

        // Detect Faces function using Task-Continuation based on LINQ Expression
        public static Task DetectFacesLinqsh(string imageSource, string imageFolderDestination)
        {
            Func<System.Drawing.Rectangle[], Image<Bgr, byte>, Image<Bgr, byte>> drawBoundries = (faces, image) =>
            {
                faces.ForAll(face => image.Draw(face, new
                    Bgr(System.Drawing.Color.BurlyWood), 3));
                return image;
            };

            // TODO finish the implementation of the "Task" operators
            // "Select" and "SelectMany" in the file "Common/Helpers/TaskComposition.cs"

            var imgTask = from image in Task.Run(() => new Image<Bgr, byte>(imageSource))
                from imageFrame in Task.Run(() => image.Convert<Gray, byte>())
                from bitmap in Task.Run(() =>
                    CascadeClassifierThreadLocal.Value.DetectMultiScale(
                        imageFrame, 1.1, 3, System.Drawing.Size.Empty)
                ).Select(faces => drawBoundries(faces, image))
                select bitmap;

            return imgTask.ContinueWith(img =>
                img.Result.Save(Path.Combine(imageFolderDestination, Path.GetFileName(imageSource))));
        }

        public static void DetectFacesPipeline(string imagesFolder)
        {
            // The refactor Detect-Face code using the parallel Pipeline
            var files = Directory.GetFiles(imagesFolder);

            Func<string, Image<Bgr, byte>> imageFn =
                (fileName) => new Image<Bgr, byte>(fileName);

            Func<Image<Bgr, byte>, Tuple<Image<Bgr, byte>, Image<Gray, byte>>> grayFn =
                image => Tuple.Create(image, image.Convert<Gray, byte>());

            Func<Tuple<Image<Bgr, byte>, Image<Gray, byte>>,
                Tuple<Image<Bgr, byte>, System.Drawing.Rectangle[]>> detectFn =
                frames => Tuple.Create(frames.Item1,
                    CascadeClassifierThreadLocal.Value.DetectMultiScale(
                        frames.Item2, 1.1, 3, System.Drawing.Size.Empty));

            Func<Tuple<Image<Bgr, byte>, System.Drawing.Rectangle[]>, Image<Bgr, byte>> drawFn =
                faces =>
                {
                    foreach (var face in faces.Item2)
                        faces.Item1.Draw(face, new Bgr(System.Drawing.Color.BurlyWood), 3);
                    return faces.Item1;
                };

            // TODO : replace one of the F# pipeline using the 
            //        C# implementation from project /Pipeline
            //        "ParallelPatterns.Pipeline<>"


            var imagePipe =
                ParallelPatterns.Pipeline<string, Image<Bgr, byte>>
                    .Create(imageFn);

            imagePipe.Then(grayFn)
                .Then(detectFn)
                .Then(drawFn);


            foreach (string fileName in files)
                imagePipe.Enqueue(fileName);
        }
    }
}