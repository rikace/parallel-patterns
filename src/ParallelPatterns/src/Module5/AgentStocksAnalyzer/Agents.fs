module Agents 

    open System
    open System.Collections.Generic
    open System.Reactive.Linq
    open Messages
    open Utils

    let stocksObservable (agent: Agent<StockAgentMessage>) =
        Observable.Interval(TimeSpan.FromMilliseconds 100.)
        |> Observable.scan(fun s i -> updatePrice s) 20m
        |> Observable.add(fun u -> agent.Post({ Price=u; Time=DateTime.Now}))

    let stockAgentObs (stockSymbol:string, chartAgent:Agent<ChartSeriesMessage>) =
        Agent<StockAgentMessage>.Start(fun inbox -> 
            stocksObservable(inbox)
            let rec loop stockPrice (chartAgent:Agent<ChartSeriesMessage>) = async {
                let! { Price=price; Time=time } = inbox.Receive()
                let message = HandleStockPrice(stockSymbol, price, time)
                chartAgent.Post(message)  
                return! loop stockPrice chartAgent }
            loop 20m chartAgent)

    // TODO :   Create a stock-Agent using the MailboxProcessor     
    //          There will be one Agent for each stock-symbol
    //          This Agent is keeping track of the price of the Stock (at least the last one), 
    //          and it updates the chart-Agent by sending the update value

    // TODO :   For Updating the price you would need a timer, possible options are
    //          1) Use the Agent timer built in (TryReceive)
    //          2) Use Reactive Extensions with Observable.Interval 
    //             and subscribe the Observer to "Post" the updated price to the Agent

    let stockAgent (stockSymbol:string, chartAgent:Agent<ChartSeriesMessage>) =
        Agent<StockAgentMessage>.Start(fun inbox -> 
            let rec loop stockPrice  = async {
                let! msg = inbox.TryReceive(750)
                match msg with
                | None ->   let newPrice = updatePrice stockPrice
                            inbox.Post({ Price=newPrice; Time=DateTime.Now })
                            return! loop newPrice 
                | Some(msg) ->
                    let { Price=price; Time=time } = msg
                    let message = HandleStockPrice(stockSymbol,price, time)
                    chartAgent.Post(message)  
                    return! loop stockPrice  }
            loop 20m )



    // TODO :   Create a coordinator-Agent using the MailboxProcessor    
    //          This agent keeps track of the subsscribed stock-Agents, one per stock symbol
    //          Use internal state to add new stock agents when requested, 
    //          or for removing existing stock-Agents when the stock symbol is removed
    //          Use the messages :  
    //              WatchStock    and   UnWatchStock

    let stocksCoordinatorAgent(lineChartingAgent:Agent<ChartSeriesMessage>) =
        Unchecked.defaultof<Agent<StocksCoordinatorMessage>>
        // add missing code here

    
    type LineSeries = {series:decimal list; color:ConsoleColor}

    let lineChartingAgent() =       
        Agent<ChartSeriesMessage>.Start(fun inbox ->
                let series =  Dictionary<string, LineSeries>()
                let rec loop() = async {
                    let! msg = inbox.Receive()
                    match msg with
                    | AddSeriesToChart(s, c) -> 
                            if not <| series.ContainsKey(s) then
                                let lineSeries = {series=[]; color=c}
                                series.Add(s, lineSeries)
                                return! loop()
                    | RemoveSeriesFromChart(s) -> 
                            if series.ContainsKey(s) then
                                let seriesToRemove = series.[s]
                                series.Remove(s) |> ignore                            
                            return! loop()
                    | HandleStockPrice(s,p, d) -> 
                            if series.ContainsKey(s) then                                
                                let seriesToUpdate = series.[s]
                                series.[s] <- { seriesToUpdate with series = p::seriesToUpdate.series }
                                
                                sprintf "Stock [%s] - Time %A - Value %M" s (d.ToString("yy.MM.dd")) p
                                |> printSeries series.[s].color 

                            return! loop() }
                loop() )