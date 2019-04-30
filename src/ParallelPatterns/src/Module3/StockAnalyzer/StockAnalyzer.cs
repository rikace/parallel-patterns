using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Net.Http;
using Functional.Async;
using static StockAnalyzer.StockUtils;

namespace StockAnalyzer
{
    public class StockAnalyzer
    {
        public static readonly string[] Stocks =
            new[] { "MSFT", "FB", "AAPL", "GOOG", "AMZN" };

        //  The Or combinator applies to falls back behavior
        Func<string, string> alphavantageSourceUrl = (symbol) =>
            $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize=full&apikey=W3LUV5WID6C0PV5L&datatype=csv";

        Func<string, string> stooqSourceUrl = (symbol) =>
            $"https://stooq.com/q/d/l/?s={symbol}.US&i=d";

        //  Stock prices history analysis
        async Task<StockData[]> ConvertStockHistory(string stockHistory)
        {
            return await Task.Run(() =>
            {
                string[] stockHistoryRows =
                    stockHistory.Split(Environment.NewLine.ToCharArray(),
                                       StringSplitOptions.RemoveEmptyEntries);
                return (from row in stockHistoryRows.Skip(1)
                        let cells = row.Split(',')
                        let date = DateTime.Parse(cells[0])
                        let open = double.Parse(cells[1])
                        let high = double.Parse(cells[2])
                        let low = double.Parse(cells[3])
                        let close = double.Parse(cells[4])
                        select new StockData(date, open, high, low, close)
                       ).ToArray();
            });
        }

        async Task<string> DownloadStockHistory(string symbol)
        {
            string url = alphavantageSourceUrl(symbol);

            var request = WebRequest.Create(url);
            using (var response = await request.GetResponseAsync()
                                              .ConfigureAwait(false))
            using (var reader = new StreamReader(response.GetResponseStream()))
                return await reader.ReadToEndAsync().ConfigureAwait(false);
        }

        async Task<Tuple<string, StockData[]>> ProcessStockHistory(string symbol)
        {
            string stockHistory = await DownloadStockHistory(symbol);
            StockData[] stockData = await ConvertStockHistory(stockHistory);
            return Tuple.Create(symbol, stockData);
        }

        public async Task AnalyzeStockHistory(string[] stockSymbols)
        {
            var sw = Stopwatch.StartNew();

            IEnumerable<Task<Tuple<string, StockData[]>>> stockHistoryTasks =
              stockSymbols.Select(stock => ProcessStockHistory(stock));

            var stockHistories = new List<Tuple<string, StockData[]>>();
            foreach (var stockTask in stockHistoryTasks)
                stockHistories.Add(await stockTask);

            DisplayStockInfo(stockHistories, sw.ElapsedMilliseconds);
        }

        async Task<string> DownloadStockHistory(string symbol,
                                                CancellationToken token)
        {
            string stockUrl = alphavantageSourceUrl(symbol);
            var request = await new HttpClient().GetAsync(stockUrl, token);
            return await request.Content.ReadAsStringAsync();
        }
        async Task AnalyzeStockHistory(string[] stockSymbols,
                               CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            //  Cancellation of Asynchronous operation manual checks
            List<Task<Tuple<string, StockData[]>>> stockHistoryTasks =
                stockSymbols.Select(async symbol =>
                {
                    var request = HttpWebRequest.Create(alphavantageSourceUrl(symbol));
                    using (var response = await request.GetResponseAsync())
                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        token.ThrowIfCancellationRequested();

                        var csvData = await reader.ReadToEndAsync();
                        var prices = await ConvertStockHistory(csvData);

                        token.ThrowIfCancellationRequested();
                        return Tuple.Create(symbol, prices.ToArray());
                    }
                }).ToList();

            await Task.WhenAll(stockHistoryTasks)
                .ContinueWith(stockData => 
                    DisplayStockInfo(stockData.Result, sw.ElapsedMilliseconds), token);
        }

        //  The Bind operator in action
        // TODO : 4.7
        // implement the bind operator respecting the top signature
        // the implementation should be full async (no blocking)
        // take a look at the Bind operator
        // replace using SelectMany and then use the Linq expression semantic (from ** in)
        async Task<Tuple<string, StockData[]>> ProcessStockHistoryBind(string symbol)
        {
            return await DownloadStockHistory(symbol)
                    .Bind(stockHistory => ConvertStockHistory(stockHistory))
                    .Bind(stockData => Task.FromResult(Tuple.Create(symbol,
                                                               stockData)));
        }

        async Task<string> DownloadStockHistory(Func<string, string> sourceStock,
                                                                    string symbol)
        {
            string stockUrl = sourceStock(symbol);
            var request = WebRequest.Create(stockUrl);
            using (var response = await request.GetResponseAsync())
            using (var reader = new StreamReader(response.GetResponseStream()))
                return await reader.ReadToEndAsync();
        }

        // TODO : 4.9
        // Process the Stock-History analysis for all the stocks in parallel
        public async Task ProcessStockHistoryParallel()
        {
            var sw = Stopwatch.StartNew();

            // TODO
            // (1) Process the stock analysis in parallel
            // When all the computation complete, then output the stock details
            // Than control the level of parallelism processing max 2 stocks at a given time
            // Suggestion, use the RequestGate class (and/or ExecuteInWithDegreeOfParallelism class)
            List<Task<Tuple<string, StockData[]>>> stockHistoryTasks = null; ;



            // (2) display the stock info
            //      DisplayStockInfo

            // (3) process each Task as they complete
            // replace point (1)
            // update the code to process the stocks in parallel and update the console (DisplayStockInfo) 
            // as the results arrive
        }

        async Task<Tuple<string, StockData[]>> ProcessStockHistoryConditional(string symbol)
        {
            Func<Func<string, string>, Func<string, Task<string>>> downloadStock =
                service => stock => DownloadStockHistory(service, stock);

            Func<string, Task<string>> googleService =
                                    downloadStock(alphavantageSourceUrl);
            Func<string, Task<string>> yahooService =
                                    downloadStock(stooqSourceUrl);

            // TODO : 4.8
            // Take a look at the operators
            // AsyncEx.Retry
            // AsyncEx.Otherwise
            // in \AsyncOperation\AsyncEx
            // 
            // Implement a reliable way to retrieve the stocks using these operators
            // Suggestion, there are 2 endpoints available: Google and Yahoo finance
            // ideally, you should use both Retry and Otherwise
            //
            // also, instead of the yahoo service you can try to load 
            // the data from the current file system and fault-back using the 
            // filesystem resource
            var _stocks = Directory.GetFiles("../../Data/Tickers");

            // TODO : try to control the degree of parallelism using "RequestGate.cs"

            return null;
        }
    }
}