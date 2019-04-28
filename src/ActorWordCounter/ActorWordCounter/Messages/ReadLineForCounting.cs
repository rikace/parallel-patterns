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

        public ReadLineForCounting(String line) { Line = line; }
    }
}
