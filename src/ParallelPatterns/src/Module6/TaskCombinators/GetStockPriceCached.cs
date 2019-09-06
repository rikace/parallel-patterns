using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Combinators;
using ParallelPatterns.Common;
using ReactiveAgent.Agents;
using static ParallelPatterns.Common.OptionHelpers;
using static Functional.Async.AsyncEx;

namespace TaskCombinators
{
    using StockCache = ImmutableDictionary<(string, DateTime), StockData>;
    public class GetStockPriceCached
    {
        public class StcokRegistry
        {
            IReplyAgent<Msg, StockData> agent;
            Func<(string, DateTime), Task<StockData>> loadStockData;

            class Msg
            {
                public string Symbol { get; set; }
                public DateTime Date { get; set; }
            }

            class LookupMsg : Msg { }
            class RegisterMsg : Msg
            {
                public StockData StockData { get; set; }
            }

            public StcokRegistry(Func<(string, DateTime), Task<StockData>> loadStockData)
            {
                this.loadStockData = loadStockData;

                this.agent = Agent.Start(StockCache.Empty, (StockCache cache, Msg msg) =>
                         Lookup(cache, (msg.Symbol, msg.Date.Date))
                         .Match(
                           some: _ => Task.FromResult(cache),
                           none: async () =>
                           {
                               var stockdata = await GetStockPrice.GeStockData(msg.Symbol, msg.Date);
                               return cache.Add((msg.Symbol, msg.Date), stockdata);
                           })
                   , (StockCache cache, Msg msg) =>
                   {
                       return Task.FromResult((cache, cache[(msg.Symbol, msg.Date)]));
                   });
            }

            static Option<T> Lookup<K, T>(IDictionary<K, T> dict, K key)
            {
                T value;
                return dict.TryGetValue(key, out value) ? Some(value) : None;
            }

            public Task<StockData> Lookup(string symbol, DateTime date)
                => agent.Ask(new LookupMsg { Symbol = symbol, Date = date })
                        .Otherwise(async () =>                        
                            await GetStockPrice.GeStockData(symbol, date)
                        );
          
        }
    }
}