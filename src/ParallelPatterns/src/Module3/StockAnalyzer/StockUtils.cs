using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StockAnalyzer
{
    public static class StockUtils
    {
        public static void DisplayStockInfo(IEnumerable<Tuple<string, StockData[]>> stockHistories, long elapsedTime)
        {
            var timeElapsed = $"Time elapsed {elapsedTime} ms";

            foreach (var s in stockHistories)
            {
                var legendText = s.Item1;

                var highest = s.Item2.OrderByDescending(f => f.High).First();
                var lowest = s.Item2.OrderBy(f => f.Low).First();

                PrintWithColor(legendText, $"highest price on date {highest.Date} - High ${highest.High} - Low ${highest.Low}");
                PrintWithColor(legendText, $"lowest price on date {lowest.Date} - Low ${lowest.High} - Low ${highest.Low}");
            }
        }

        static void PrintWithColor(string stock, string text)
        {
            var color = Console.ForegroundColor;
            if (stock == "MSFT")
                Console.ForegroundColor = ConsoleColor.Green;
            else if (stock == "FB")
                Console.ForegroundColor = ConsoleColor.Blue;
            else if (stock == "AAPL")
                Console.ForegroundColor = ConsoleColor.Red;
            else if (stock == "GOOG")
                Console.ForegroundColor = ConsoleColor.Magenta;
            else if (stock == "AMZN")
                Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{stock} - {text}");
            Console.ForegroundColor = color;
        }
    }

}
