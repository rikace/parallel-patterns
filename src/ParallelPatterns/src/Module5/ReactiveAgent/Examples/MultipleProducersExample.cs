using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ReactiveAgent.Examples
{
    class MultipleProducersExample
    {
        // Asynchronous producer/consumer using TPL Dataflow
        BufferBlock<int> buffer = new BufferBlock<int>(
            new DataflowBlockOptions { BoundedCapacity = 10 });

        async Task Produce(IEnumerable<int> values)
        {
            foreach (var value in values)
                await buffer.SendAsync(value); ;
        }

        async Task MultipleProducers(params IEnumerable<int>[] producers)
        {
            await Task.WhenAll(
                    (from values in producers select Produce(values)).ToArray())
                .ContinueWith(_ => buffer.Complete());
        }

        async Task Consumer(Action<int> process)
        {
            while (await buffer.OutputAvailableAsync())
                process(await buffer.ReceiveAsync());
        }

        public async Task Run()
        {
            IEnumerable<int> range = Enumerable.Range(0, 100);

            await Task.WhenAll(MultipleProducers(range, range, range),
                Consumer(n => Console.WriteLine($"value {n} - ThreadId{Thread.CurrentThread.ManagedThreadId}")));
        }
    }

}
