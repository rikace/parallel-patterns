using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorWordCounter.Messages
{
    public class TotalWordCount
    {
        public readonly Int32 TotalCount;

        public TotalWordCount(Int32 total) { TotalCount = total; }
    }
}
