using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorWordCounter.Messages
{
    public class WordCount
    {
        public readonly String TheWord;
        public readonly Int32 Count;

        public WordCount(String word, Int32 count) { TheWord = word; Count = count; }
    }
}
