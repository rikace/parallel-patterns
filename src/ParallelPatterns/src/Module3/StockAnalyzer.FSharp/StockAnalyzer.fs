module StockAnalyzer

open System
open System.IO
open System.Net
open System.Diagnostics
open FunctionalConcurrency

type StockData = {date:DateTime;open':float;high:float;low:float;close:float}

let Stocks = ["MSFT";"FB";"AAPL";"AMZN"; "GOOG"]

let displayStockInfo (symbolInfo:(string * StockData[])seq) elapsedTime =  
    let symbolInfo = symbolInfo |> Seq.map(fun (n,sinfo) ->  
                let stockData = 
                    sinfo 
                    |> Seq.map(fun s -> 
                        StockAnalyzer.StockData(s.date,s.open',s.high,s.low,s.close))
                    |> Seq.toArray
                (n, stockData))
    StockAnalyzer.StockUtils.DisplayStockInfo(symbolInfo,elapsedTime)


let convertStockHistory (stockHistory:string) = async {
    let stockHistoryRows =
        stockHistory.Split(
            Environment.NewLine.ToCharArray(),
            StringSplitOptions.RemoveEmptyEntries)
    return
        stockHistoryRows
        |> Seq.skip 1
        |> Seq.map(fun row -> row.Split(','))
        // this is a guard against bad CSV row formatting when for example the stock index
        |> Seq.filter(fun cells -> cells |> Array.forall(fun c -> not <| (String.IsNullOrWhiteSpace(c) || (c.Length = 1 && c.[0] = '-'))))
        |> Seq.map(fun cells ->
            {
                date = DateTime.Parse(cells.[0]).Date
                open' = float(cells.[1])
                high = float(cells.[2])
                low = float(cells.[3])
                close = float(cells.[4])
            })
        |> Seq.toArray
}

let googleSourceUrl symbol =
    sprintf "https://www.alphavantage.co/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol=%s&outputsize=full&apikey=W3LUV5WID6C0PV5L&datatype=csv" symbol

let yahooSourceUrl symbol =
    sprintf "https://stooq.com/q/d/l/?s=%s.US&i=d" symbol

let downloadStockHistory symbol = async {
    let url = googleSourceUrl symbol
    let req = WebRequest.Create(url)
    let! resp = req.AsyncGetResponse()
    use reader = new StreamReader(resp.GetResponseStream())
    return! reader.ReadToEndAsync()
}

let processStockHistory symbol = async {
    let! stockHistory = downloadStockHistory symbol
    let! stockData = convertStockHistory stockHistory
    return (symbol, stockData)
}

// TODO : 4.9
// Process the Stock-History analysis for all the stocks in parallel
let processStockHistoryParallel() =
    let sw = Stopwatch.StartNew()

    // TODO
    // (1) Process the stock analysis in parallel
    // When all the computation complete, then output the stock details
    // Than control the level of parallelism processing max 2 stocks at a given time
    // Suggestion, use the RequestGate class
    let stockHistory = Unchecked.defaultof<Async<_>>
    
    // (2) display the stock info
    //      DisplayStockInfo

    // (3) process each Task as they complete
    // replace point (1)
    // update the code to process the stocks in parallel and update the console (DisplayStockInfo) 
    // as the results arrive
    ()

        
let rocessStockHistoryConditional(symbol:string) =

        // googleService
        let downloadStock = downloadStockHistory symbol

        // implement a service that dowloads the symbol history from Yahoo
        let downloadStock' = downloadStockHistory symbol

      
        // TODO : 4.8
        // Take a look at the operators
        // AsyncEx.Retry
        // AsyncEx.Otherwise
        // in \AsyncOperation\AsyncEx
        // Implement a reliable way to retrieve the stocks using these operators
        // Suggestion, there are 2 endpoints available Google and Yahoo finance
        // ideally, you should use both Retry and Otherwise
        //
        // also, instead of the yahoo service you can try to load 
        // the data from the current file system and faulback using the 
        // filesystem resource
        let _stocks = Directory.GetFiles("../../Data/Tickers")

        // TODO : try to control the degree of parallelism, 
        // "RequestGate" is also a good option, but not the only one
        // StockAnalyzer/RequestGate.cs
        ()
         
let analyzeStockHistory() =
    let time = Stopwatch.StartNew()
    let stockInfo =
        Stocks
        |> Seq.map (processStockHistory)
        |> Async.Parallel
        |> Async.RunSynchronously
    displayStockInfo stockInfo time.ElapsedMilliseconds
    
