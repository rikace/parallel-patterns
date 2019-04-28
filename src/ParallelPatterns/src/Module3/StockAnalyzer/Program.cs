using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.CS
{

    class Program
    {
        static void Main(string[] args)
        {
            var stocks = Directory.GetFiles("../../Data/Tickers");

            //  Cancellation of Asynchronous Task
            CancellationTokenSource cts = new CancellationTokenSource();

            var stockAnalyzer = new StockAnalyzer();

            Task.Factory.StartNew(async () => await stockAnalyzer.ProcessStockHistoryParallel(), cts.Token);

            Console.ReadLine();
            cts.Cancel();
        }
    }
}



