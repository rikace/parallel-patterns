using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActorWordCounter.Writers
{
    public interface IWriteStuff
    {
        void WriteLine(String format, params Object[] args);
    }
}
