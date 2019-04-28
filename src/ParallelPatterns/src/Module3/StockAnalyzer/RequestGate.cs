using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StockAnalyzer.CS
{
    public class RequestGate
    {
        SemaphoreSlim semaphore;
        public RequestGate(int count) =>
            semaphore = new SemaphoreSlim(initialCount: count, maxCount: count);

        public async Task<IDisposable> AsyncAcquire(TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            // TODO 
            // implement the logic to coordinate the access to resources 
            // using "semaphore". Keep async semantic for the "acquire" and "release" of the hanld3
            throw new Exception("No implemented");
        }
    }
}
