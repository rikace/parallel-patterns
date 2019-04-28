using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using ReactiveStockTickers;

namespace RxConcurrentStockTickers
{
    class Program
    {
        static void Main(string[] args)
        {
            ObservableDataStreams.RxStream();

            // ThrottlingStream.Start();

            Console.ReadLine();
        }
    }
}