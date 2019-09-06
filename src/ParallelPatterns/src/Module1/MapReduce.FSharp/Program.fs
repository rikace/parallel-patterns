open System
open System.Linq
open MapReduce
open KMeans.Data
open KMeans.FsPSeq
open System.Collections.Generic

[<EntryPoint>]
[<STAThread>]
let main argv =

    let initialCentroidsSet = data |> getRandomCentroids 11

    // TODO :
    //  complete the Map-Reduce function in MapReduce.FsPSeq.fs file
    let run () = kmeans data dist initialCentroidsSet

    run ()
    
    
    
    Console.ReadLine() |> ignore
        
    let map = fun (fileName:string, fileContent:string) ->
                let l = new List<string * int>()
                let wordDelims = [|' ';',';';';'.';':';'?';'!';'(';')';'\n';'\t';'\f';'\r';'\b'|]
                fileContent.Split(wordDelims) |> Seq.iter (fun word -> l.Add((word, 1)))
                l :> seq<string * int>

    let reduce = fun key (values:seq<int>) -> [values |> Seq.sum] |> seq<int>
    let partitionF = fun key M -> abs(key.GetHashCode()) % M 
    let testInput = ["File1", "I was going to the airport when I saw someone crossing";
                     "File2", "I was going home when I saw you coming toward me"] |> List.toSeq
                 
    // TODO :
    //  complete the Map-Reduce function in MapReduce.fs file         
    let output = MapReduce.MapReduceFsPSeq.mapReduce testInput map reduce 2 2
    

    Console.ReadLine() |> ignore
    0 // return an integer exit code