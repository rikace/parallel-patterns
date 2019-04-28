using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ReactiveStockTickers
{
    public static class Utils
    {
        public static void PrintStockData(IList<StockData> data)
        {
            var symbol = data.First().Symbol;
            ConsoleColor symbolColor = GetColorForSymbol(symbol.Substring(0, symbol.IndexOf('.')));
            using (new ColorPrint(symbolColor))
                foreach (var x in data)
                    Console.WriteLine($"{x.Symbol}({x.Date}) = {x.High}-{x.Low} {x.Open}/{x.Close}");
        }

        public static ConsoleColor GetColorForSymbol(string symbol)
        {
            switch (symbol)
            {
                case "msft":
                    return ConsoleColor.Cyan;
                case "aapl":
                    return ConsoleColor.Red;
                case "fb":
                    return ConsoleColor.Magenta;
                case "goog":
                    return ConsoleColor.Yellow;
                case "amzn":
                    return ConsoleColor.Green;
                default:
                    return Console.ForegroundColor;
            }
        }

        public class ColorPrint : IDisposable
        {
            private ConsoleColor old;
            public ColorPrint(ConsoleColor color)
            {
                old = Console.ForegroundColor;
                Console.ForegroundColor = color;
            }
            public void Dispose()
            {
                Console.ForegroundColor = old;
            }
        }
    }
}
