using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;

namespace ReactiveStockTickers
{
    class FileLinesStream<T>
    {
        public FileLinesStream(string filePath, Func<string, T> map)
        {
            _filePath = filePath;
            _map = map;
            _data = new List<T>();
        }

        private string _filePath;
        private List<T> _data;
        private Func<string, T> _map;

        public IEnumerable<T> GetLines()
        {
            const string tickerPath = "../../Data/Tickers";
            using (var stream = File.OpenRead(Path.Combine(tickerPath, _filePath)))
            using (var reader = new StreamReader(stream))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var value = _map(line);
                    if (value != null)
                        _data.Add(value);
                }
            }
            _data.Reverse();
            while (true)
                foreach (var item in _data)
                    yield return item;
        }

        // TODO : enable Task scheduler to generate the stream of events concurrently
        public IObservable<T> ObserveLines() => GetLines().ToObservable();
    }

}
