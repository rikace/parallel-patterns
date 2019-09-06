using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncOperations
{
    public class RequestGate
    {
        readonly SemaphoreSlim semaphore;

        public RequestGate(int count) =>
            semaphore = new SemaphoreSlim(initialCount: count, maxCount: count);

        public async Task<IDisposable> AsyncAcquire(TimeSpan timeout,
            CancellationToken cancellationToken = new CancellationToken())
        {
            // TODO 
            // implement the logic to coordinate the access to resources 
            // using "semaphore". Keep async semantic for the "acquire" and "release" of the handle
            // throw new Exception("No implemented");


            throw new Exception("couldn't acquire a semaphore");
        }
    }
}