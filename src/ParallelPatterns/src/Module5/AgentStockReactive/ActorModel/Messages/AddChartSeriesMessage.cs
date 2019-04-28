using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveStock.ActorModel.Messages
{
    class AddChartSeriesMessage : ChartSeriesMessage
    {
        public string StockSymbol { get; }
        public ConsoleColor Color { get; }

        public AddChartSeriesMessage(string stockSymbol, ConsoleColor color)
        {
            StockSymbol = stockSymbol;
            Color = color;
        }
    }
}
