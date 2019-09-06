using Helpers;
using System;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Pipeline
{
    class Program
    {
        public static void TestFilteringPipeline()
        {
            //Generate the source data.
            var source = new BlockingCollection<int>();

            Parallel.For(0, 100, (data) =>
            {
                if(source.TryAdd(data))
                    Console.WriteLine("added {0} to source data", data);
            });

            source.CompleteAdding();

            // calculate the square 
            var calculateFilter = new PipelineFilter<int, int>
            (
                source,
                (n) => n * n,
                "calculateFilter"
            );

            //Convert ints to strings
            var convertFilter = new PipelineFilter<int, string>
            (
                calculateFilter.m_outputData,
                (s) => String.Format("{0}", s),
                "convertFilter"
            );

            // Displays the results
            var displayFilter = new PipelineFilter<string, string>
            (
                convertFilter.m_outputData,
                (s) => Console.WriteLine("The final result is {0}", s),
                "displayFilter");

            // Start the pipeline
            try
            {
                Parallel.Invoke(
                    () => calculateFilter.Run(),
                    () => convertFilter.Run(),
                    () => displayFilter.Run()
                );
            }
            catch (AggregateException aggregate)
            {
                foreach (var exception in aggregate.InnerExceptions)
                    Console.WriteLine(exception.Message + exception.StackTrace);
            }

            Console.ReadLine();
        }
    

        static void Main(string[] args)
        {
            BenchPerformance.Time("Test", () =>
            {
                int sum = 0;
                for (int i = 0; i < 100; i++)
                {
                    sum += i;
                }      
            }, 10);


            Console.WriteLine("Hello World!");
            Console.ReadLine();
        }
    }
}
