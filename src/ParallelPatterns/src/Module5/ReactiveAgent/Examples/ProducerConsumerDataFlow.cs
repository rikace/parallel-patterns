using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ReactiveAgent.Examples
{
    class DataflowBufferBlock
    {
        // Simple Producer Consumer using TPL Dataflow BufferBlock
        BufferBlock<int> buffer = new BufferBlock<int>();

        async Task Producer(IEnumerable<int> values)
        {
            foreach (var value in values)
                buffer.Post(value);
            buffer.Complete();
        }
        async Task Consumer(Action<int> process)
        {
            while (await buffer.OutputAvailableAsync())
                process(await buffer.ReceiveAsync());

        }
        public async Task Run()
        {
            IEnumerable<int> range = Enumerable.Range(0, 100);
            await Task.WhenAll(Producer(range), Consumer(n =>
                Console.WriteLine($"value {n}")));
        }
    }

}
