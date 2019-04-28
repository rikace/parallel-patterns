module Messages

open System

type StocksCoordinatorMessage =
    | WatchStock of string * ConsoleColor
    | UnWatchStock of string

type ChartSeriesMessage =
    | AddSeriesToChart of string * ConsoleColor
    | RemoveSeriesFromChart of string 
    | HandleStockPrice of string * decimal * DateTime 

type StockAgentMessage = { Price:decimal; Time:DateTime }

type StockPriceLookupMessage =
    | RefreshStockPrice of AsyncReplyChannel<decimal * DateTime>

type FlipToggleMessage =
    | FlipToggle 
