using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorWordCounter.Messages
{
    public class ReadLineForCounting
    {
        public readonly String Line;
        public readonly Int32 LineNumber;

        public ReadLineForCounting(Int32 lineNumber, String line)
        {
            LineNumber = lineNumber;
            Line = line;
        }
    }
}
