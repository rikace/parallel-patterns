using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka.Actor;
using ActorWordCounter.Actors;
using ActorWordCounter.Messages;
using ActorWordCounter.Writers;
using ActorWordCounter.Readers;

namespace ActorWordCounter
{
    class Program
    {
        static void Main(string[] args)
        {
            var writer = new ConsoleWriter();
            var reader = new ConsoleReader();
            var file = PrintInstructionsAndGetFile(reader, writer);
            if (file == null)
            {
                return;
            }

            var system = ActorSystem.Create("helloAkka");

            var counter = system.ActorOf(CountSupervisor.Create(writer), "supervisor");
            counter.Tell(new StartCount(file));

            reader.ReadLine();
        }

        private static String PrintInstructionsAndGetFile(IReadStuff reader, IWriteStuff writer)
        {
            writer.WriteLine("Word counter.  Select the document to count:");
            writer.WriteLine(" (1) Magna Carta");
            writer.WriteLine(" (2) Declaration of Independence");
            var choice = reader.ReadLine();
            String file = AppDomain.CurrentDomain.BaseDirectory + @"\Files\";

            if (choice.Equals("1"))
            {
                file += @"MagnaCarta.txt";
            }
            else if (choice.Equals("2"))
            {
                file += @"DeclarationOfIndependence.txt";
            }
            else
            {
                writer.WriteLine("Invalid -- bye!");
                return null;
            }

            return file;
        }
    }
}
