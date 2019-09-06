using System;

namespace ConsoleTaskEx
{
    // using ImageDetection;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {

        static void Main(string[] args)
        {
            var sourceImages = "../../../../../Data/Images";
            var destination = "./Images/Output";
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);

            ImageProcessing imageProc = new ImageProcessing(sourceImages, destination);
            
            // TODO :
            //      try different concurrent implementations
            //      look the file /ImageDetection/FaceDetection.cs, there are 
            //      some method that you can use
            //
            //      implement the pipeline (then component used in /ImageDetection/FaceDetection.cs) 

            imageProc.RunContinuation().Wait();
            
            // imageProc.RunTransformer().Wait();
            
            // imageProc.RunPipeline();
            
            Console.WriteLine("Completed");
            Console.ReadLine();

            Console.WriteLine("Hello World!");
        }
    }
}