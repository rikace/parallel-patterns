using Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParallelFilterMap
{
    public static class ParallelEx
    {
        // TODO
        // Parallel FilterMap operator
        // Executes a map operation, converting an input list into an output list, in parallel.
        // Filters an input list, running a predicate over each element of the input.
        // A new list containing all those elements from the input that passed the filter and transformed using the transform function
        public static TOutput[] FilterMap<TInput, TOutput>(this IList<TInput> input, Func<TInput, Boolean> predicate,
            Func<TInput, TOutput> transform, ParallelOptions parallelOptions = null)
        {
            if (input == null) throw new ArgumentNullException("input");
            if (transform == null) throw new ArgumentNullException("transform");
            if (predicate == null) throw new ArgumentNullException("predicate");
            parallelOptions = parallelOptions ?? new ParallelOptions();
            var atomResult = new Atom<ImmutableList<List<TOutput>>>(ImmutableList<List<TOutput>>.Empty);


            // TODO  
            Parallel.ForEach(Partitioner.Create(0, input.Count),
                parallelOptions,
                () => new List<TOutput>(),
                delegate (Tuple<int, int> range, ParallelLoopState state, List<TOutput> localList)
                {
                    // TODO 
                    // run the sub loop using the "range" limits
                    // use the "predicate" to filter the item 
					
				
                    return localList;

                }, localList =>
                {
                    // TODO apply functionality to last step
                    // aggregate the a collection with all the results
                    // this collections is that passed as output of the FilterMap functions
                    // Suggestion, use an Immutable collection and look into the API exposed by the "ImmutableInterlocked" class

                    // add code missing here
                });

            return null;

        }
    }
}

