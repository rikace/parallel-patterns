using ReactiveStockTickers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using static ReactiveStockTickers.Utils;

namespace RxConcurrentStockTickers
{
    public static class ObservableDataStreams
    {
        public static IObservable<StockData> ObservableStreams
            (this IEnumerable<string> filePaths, Func<string, string, StockData> map, int delay = 50)
        {
            var flStreams =
                filePaths
                     .Select(x => new FileLinesStream<StockData>(x, row => map(x, row)))
                     .ToList();
            return
                flStreams
                    .Select(x =>
                    {
                        var startData = new DateTime(2001, 1, 1);
                        return Observable
                                .Interval(TimeSpan.FromMilliseconds(delay))
                                // TODO : Combine the "Observable Interval" stream with the 
                                //        x.ObserveLines() stream to increment the "stock.Date"
                                //        EX: stock.Date = startData + TimeSpan.FromDays(tick);
                                //        The combined stream should return the Stock type
                                .Select(_ => default(StockData)); // <= remove this line (compilation purpose)
                                
                    }
                    )
                    // TODO :
                    //      Merge the streams of StockData
                    //      Investigate if concurrency (Task Scheduler) is enable
                    //      otherwise enbale the parallelism
                    //      In this case the "TaskPoolScheduler.Default" could help
                    //      The concurrency could also be anabled at the source level
                    //      where the stream of event is generated "FileLinesStream"
                    .First(); // << Remove this code "First()" and complete TODO
        }

        public static void RxStream()
        {
            string[] stockFiles = new string[] { "aapl.csv", "amzn.csv", "fb.csv", "goog.csv", "msft.csv" };
            var sw = Stopwatch.StartNew();

            Task.Factory.StartNew(() =>
            {
                stockFiles
                    .ObservableStreams(StockData.Parse)
                    
                    // TODO :
                    //      try different debauncing operator to reduce
                    //      or tame back-pressure (Ex Buffer, Throttle...)
                    .GroupBy(stock => stock.Symbol)
                    .SelectMany(group => group)
                    .Subscribe(x => print(x));
            });
        }

        static void print(IList<StockData> stocks) => printAction.Post(stocks);
        static void print(StockData stock) => printAction.Post(new List<StockData> { stock });

        private static ActionBlock<IList<StockData>> printAction = new ActionBlock<IList<StockData>>(data => PrintStockData(data));

    }
}