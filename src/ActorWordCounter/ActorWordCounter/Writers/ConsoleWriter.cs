using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorWordCounter.Writers
{
    public class ConsoleWriter : IWriteStuff
    {
        public void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
    }
}
