using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveAgent.Agents;
using ReactiveStock.ActorModel.Messages;

namespace ReactiveStock.ActorModel.Actors.UI
{
   public class LineChartingActor 
    {
        private readonly Dictionary<string, (List<decimal>, ConsoleColor)> _series;

        public IAgent<ChartSeriesMessage> Actor { get; private set; }

        public LineChartingActor()
        {
            _series = new Dictionary<string, (List<decimal>, ConsoleColor)>();

            Actor = Agent.Start<ChartSeriesMessage>(message =>
                {
                    switch (message)
                    {
                        case AddChartSeriesMessage msg:
                            AddSeriesToChart(msg);
                            break;
                        case RemoveChartSeriesMessage msg:
                            RemoveSeriesFromChart(msg);
                            break;
                        case StockPriceMessage msg:
                            HandleNewStockPrice(msg);
                            break;
                        default:
                            throw new ArgumentException(
                               message: "message is not a recognized",
                               paramName: nameof(message));
                    }
                });
        }

        private void AddSeriesToChart(AddChartSeriesMessage message)
        {
            if (!_series.ContainsKey(message.StockSymbol))
            {
                var newLineSeries = (new List<decimal>(), message.Color);
                _series.Add(message.StockSymbol, newLineSeries);
            }
        }

        private void RemoveSeriesFromChart(RemoveChartSeriesMessage message)
        {
            if (_series.ContainsKey(message.StockSymbol))
            {
                var seriesToRemove = _series[message.StockSymbol];
                _series.Remove(message.StockSymbol);
            }
        }

        private void HandleNewStockPrice(StockPriceMessage message)
        {
            if (_series.ContainsKey(message.StockSymbol))
            {
                var stockInfo = _series[message.StockSymbol];

                var series = stockInfo.Item1;
                var color = stockInfo.Item2;
                var temp = Console.ForegroundColor;
                try
                {
                    Console.ForegroundColor = color;
                    Console.WriteLine($"Stock [{message.StockSymbol}' - Date [{message.Date.ToString("yy.MM.dd")}] - Value [{message.StockPrice}]");
                }
                finally
                {
                    Console.ForegroundColor = temp;
                }
            }
        }
    }
}
