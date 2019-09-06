namespace Pipeline
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PipelineFilter<TInput, TOutput>
    {
        Func<TInput, TOutput> m_function = null;
        public BlockingCollection<TInput> m_inputData = null;
        public BlockingCollection<TOutput> m_outputData = null;
        Action<TInput> m_outputAction = null;
        public string Name { get; private set; }

        public PipelineFilter(BlockingCollection<TInput> input, Func<TInput, TOutput> processor, string name)
        {
            m_inputData = input;
            // no buffer
            m_outputData = new BlockingCollection<TOutput>();

            m_function = processor;
            Name = name;
        }

        //used for final endpoint 
        public PipelineFilter(BlockingCollection<TInput> input, Action<TInput> renderer, string name)
        {
            m_inputData = input;
            m_outputAction = renderer;
            Name = name;
        }

        public void Run()
        {
            Console.WriteLine("filter {0} is running", this.Name);
            while (!m_inputData.IsCompleted)
            {
                TInput receivedItem;
                if (m_inputData.TryTake(out receivedItem, 50))
                {
                    if (m_outputData != null)
                    {
                        TOutput outputItem = m_function(receivedItem);
                        m_outputData.TryAdd(outputItem);
                        Console.WriteLine("{0} sent {1} to next filter", this.Name, outputItem);
                    }
                    else
                    {
                        m_outputAction(receivedItem);
                    }
                }
                else
                    Console.WriteLine("Could not get data from previous filter");
            }

            if (m_outputData != null)
            {
                m_outputData.CompleteAdding();
            }
        }
    }
}