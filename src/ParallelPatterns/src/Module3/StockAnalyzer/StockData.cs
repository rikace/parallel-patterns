using System;
using System.Collections.Generic;
using System.Text;

namespace StockAnalyzer
{
    public struct StockData
    {
        public StockData(DateTime date, double open, double high, double low, double close)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
        }

        public DateTime Date { get; }
        public Double Open { get; }
        public Double High { get; }
        public Double Low { get; }
        public Double Close { get; }
    }

}
