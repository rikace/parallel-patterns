using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonHelpers
{
    public static class Instrumentation
    {
        public static T Time<T>(string op, Func<T> f)
        {
            var sw = new Stopwatch();
            sw.Start();

            T t = f();

            sw.Stop();
            Console.WriteLine($"{op} took {sw.ElapsedMilliseconds}ms");
            return t;
        }

        public static T Trace<T>(string op, Func<T> f)
        {
            Console.WriteLine($"Entering {op}");
            T t = f();
            Console.WriteLine($"Leaving {op}");
            return t;
        }

        public static T Trace<T>(Action<string> log, string op, Func<T> f)
        {
            log($"Entering {op}");
            T t = f();
            log($"Leaving {op}");
            return t;
        }
    }
}
