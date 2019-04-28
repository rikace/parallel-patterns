#r "../../packages/NETStandard.Library/build/netstandard2.0/ref/netstandard.dll"
#r "../../packages/System.Reactive/lib/netstandard2.0/System.Reactive.dll"
#r "../../packages/System.Reactive.Linq/lib/netstandard2.0/System.Reactive.Linq.dll"
#r "../../packages/System.Reactive.Interfaces/lib/netstandard2.0/System.Reactive.Interfaces.dll"
//#load "../../../.paket/load/netstandard2.0/main.group.fsx"

open System
open System.Reactive
open System.Reactive.Linq
open System.Reactive.Subjects
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic

module Observable =
    let scan (accumulator:'a->'a->'a)  source =
        Observable.Scan(source, Func<'a,'a,'a> accumulator)

    let scanInit (init:'TAccumulate) (accumulator) (source:IObservable<'Source>) : IObservable<'TAccumulate> =
        Observable.Scan( source, init, Func<'TAccumulate,'Source,'TAccumulate> accumulator )

    let subscribe(onNext: 'T -> unit) (observable: IObservable<'T>) =
          observable.Subscribe(Action<_> onNext)

type ThreadSafeRandom private () =
    static let random =
            new ThreadLocal<Random>(fun () -> new Random(Guid.NewGuid().ToString().GetHashCode()))
    static member NextDecimalValue () = random.Value.Next(-5, 5) |> float

type Stock =
  { Symbol : string
    LastPrice : float
    Price : float }
   member x.Change = x.Price - x.LastPrice
   member x.UpdatePrice price = {x with LastPrice = x.Price; Price = price }

   static member CreateStock (symbol : string) price =
        { Symbol = symbol
          LastPrice = price
          Price = price }

   member x.PercentChange = double (Math.Round(x.Change / x.Price, 4))

   member x.Update() =
    let r = ThreadSafeRandom.NextDecimalValue()
    if r > 0.1 then x
    else
        let rnd' = Random(int (Math.Floor(x.Price)))
        let percenatgeChange = rnd'.NextDouble() * 0.002
        let change =
            let change = Math.Round(x.Price * percenatgeChange, 2)
            if (rnd'.NextDouble() > 0.51) then change
            else -change
        let price = x.Price + change
        { x with  LastPrice = price
                  Price = price }



let msft = Stock.CreateStock("MSFT") 95.
let amzn = Stock.CreateStock("AMZ") 197.
let goog = Stock.CreateStock("GOOG") 513.

let seqStocks = [msft;amzn;goog]

let sb = new Subject<Stock>()


let updatedStocks (stocks:Stock list) =
        stocks |> List.map(fun s -> s.Update())

let obs = { new IObserver<Stock> with
                member x.OnNext(s) = printfn "Stock %s - price %4f" s.Symbol s.Price
                member x.OnCompleted() = printfn "Completed"
                member x.OnError(exn) = ()   }

let dispose = sb.Subscribe(obs)


let stocksObservable =
    Observable.Interval(TimeSpan.FromMilliseconds (ThreadSafeRandom.NextDecimalValue() * 100.))
    |> Observable.scanInit seqStocks (fun s o -> updatedStocks s)
    |> Observable.subscribe(fun s -> s |> List.iter (sb.OnNext))



sb.OnCompleted()
stocksObservable.Dispose()
