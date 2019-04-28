namespace MapReduce

module MapReduceSequential =

    let mapReduce
            (inputs:seq<'in_value>)
            (map:'in_value -> seq<'out_key * 'out_value>)
            (reduce:'out_key -> seq<'out_value> -> 'reducedValues)
            M R =
        inputs
        |> Seq.collect (map)
        |> Seq.groupBy (fst)
        |> Seq.map (fun (key, items) ->
            items
            |> Seq.map (snd)
            |> reduce key)
        |> Seq.toList

module ParellelMapReduce = 

    open ParallelSeq
    open System.Linq
    open System.Collections.Generic

    //  Implementation of mapF function for the first phase of the MapReduce pattern
    let mapF  M (map:'in_value -> seq<'out_key * 'out_value>)
                (inputs:seq<'in_value>) =
        inputs
        |> PSeq.withExecutionMode ParallelExecutionMode.ForceParallelism  
        |> PSeq.withDegreeOfParallelism M  
        |> PSeq.collect (map)  
        |> PSeq.groupBy (fst) 
        |> PSeq.toList 

    // Implementation of reduceF function for the second phase of the MapReduce pattern
    let reduceF  R (reduce:'key -> seq<'value> -> 'reducedValues)
                   (inputs:('key * seq<'key * 'value>) seq) =
        inputs
        |> PSeq.withExecutionMode ParallelExecutionMode.ForceParallelism  
        |> PSeq.withDegreeOfParallelism R  
        |> PSeq.map (fun (key, items) ->  
            items
            |> Seq.map (snd)  
            |> reduce key)    
        |> PSeq.toList

    // TODO : implement the MapReduce pattern
    //        - then add an optional argument to print thread-safely the output (could be a function) 
    //        - Suggestion : try composing the mapF and reduce functions
    let mapReduce
            (inputs:seq<'in_value>)
            (map:'in_value -> seq<'out_key * 'out_value>)
            (reduce:'out_key -> seq<'out_value> -> 'reducedValues)
            M R =

        // TODO : 2.11
        // Complete the map reduce composing the function "mapF" and "reduceF"
        // suggestion, use the ">>" composition operator
        inputs |> // compose map and reduce here
                  (id) // <= remove this after implementation


