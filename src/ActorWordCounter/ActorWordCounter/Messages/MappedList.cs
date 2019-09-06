using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorMapReduceWordCount.Messages
{
    public class MappedList
    {
        public readonly Dictionary<String, Int32> LineWordCount;
        public readonly Int32 LineNumber;

        public MappedList(Int32 lineNumber, Dictionary<String, Int32> wordCounts)
        {
            LineNumber = lineNumber; LineWordCount = wordCounts;
        }
    }
}