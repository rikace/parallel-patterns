using System;
using Pipeline;

namespace ConsoleTaskEx
{
    using ImageDetection;
    using System.IO;
    using System.Threading.Tasks;

    class Program
    {

        private static void RunFaceDetectionsPipeline()
        {
            // TODO
            // Complete Pipeline            
            var images = Directory.GetFiles("../../Data/Images");

            var destination = "./Images/Output";
            if (!Directory.Exists(destination))
                Directory.CreateDirectory(destination);




            // TODO :
            //      try different concurrent implementations
            //      look the file /ImageDetection/FaceDetection.cs, there are 
            //      some method that you can use
            //
            //      implement the pipeline (then component used in /ImageDetection/FaceDetection.cs) 
            foreach (var image in images)
            {
                Console.WriteLine($"Processing {Path.GetFileNameWithoutExtension(image)}");

                FaceDetection.DetectFaces(image, destination);
            }
        }

        static void Main(string[] args)
        {
            // TODO 
            // complete ProducerConsumer
            ProducerConsumer pc = new ProducerConsumer();
            pc.Run();

            
            // TODO 
            // RunFaceDetectionsPipeline();
            
            Console.WriteLine("Completed");
            Console.ReadLine();
            
            
        }
    }
}
