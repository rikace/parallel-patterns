using Akka;
using Akka.Streams;
using Akka.Streams.Dsl;
using RxConcurrentStockTickers;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static ReactiveStockTickers.Utils;
using Akka.Actor;

namespace ReactiveStockTickers
{
    public class ThrottlingStream
    {
        public static void Start()
        {
            using (var sys = ActorSystem.Create("ReactiveStream"))
            {
                Console.OutputEncoding = System.Text.Encoding.UTF8;
                Console.ForegroundColor = ConsoleColor.Cyan;

                Console.WriteLine("Press Enter to Start");
                Console.ReadLine();

                string[] stockFiles = new string[] { "aapl.csv", "amzn.csv", "fb.csv", "goog.csv", "msft.csv" };

                using (var mat = sys.Materializer())
                {
                    var enStocks = stockFiles.Select(stock => new FileLinesStream<(string, string)>(stock, s => (stock, s)));
                    var en = enStocks.Select(x => x.ObserveLines()).Aggregate((o1, o2) => o1.Merge(o2));
                    var stocksSource = Source.FromEnumerator(() => en.GetEnumerator());
                    
                    var graph = CreateRunnableGraph(stocksSource);
                    graph.Run(mat);

                    Console.WriteLine("Press Enter to exit");
                    Console.ReadLine();
                }
            }

            IRunnableGraph<TMat> CreateRunnableGraph<TMat>(Source<(string, string), TMat> stocksSource)
            {
                var writeSink = Sink.ForEach<StockData>(x =>
                {
                    var symbol = x.Symbol;
                    ConsoleColor symbolColor = GetColorForSymbol(symbol.Substring(0, symbol.IndexOf('.')));
                    using (new ColorPrint(symbolColor))
                        Console.WriteLine($"{x.Symbol}({x.Date}) = {x.High}-{x.Low} {x.Open}/{x.Close}");
                });

                var formatData = Flow.Create<(string, string)>().Select(r => StockData.Parse(r.Item1, r.Item2));

                var graph = GraphDsl.Create(b =>
                {
                    var broadcast = b.Add(new Broadcast<(string, string)>(1));
                    var merge = b.Add(new Merge<StockData>(1));
                    b.From(broadcast.Out(0))
                        .Via(formatData)
                        //  .Throttle(10, TimeSpan.FromSeconds(1), 1, ThrottleMode.Shaping))                    
                        .To(merge.In(0));

                    return new FlowShape<(string, string), StockData>(broadcast.In, merge.Out);
                });
                return stocksSource.Via(graph)//.GroupBy(10, stock => stock.Symbol)
                          .To(writeSink);
            }
        }
    }
}
