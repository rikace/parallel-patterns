using System;
using System.Collections.Generic;

namespace Helpers
{
    public class MultiDisposables : IDisposable
    {
        private readonly IEnumerable<IDisposable> _contents;

        public MultiDisposables(params IDisposable[] items)
        {
            _contents = items;
        }

        public void Dispose()
        {
            foreach (var item in _contents)
                item?.Dispose();
        }
    }
}