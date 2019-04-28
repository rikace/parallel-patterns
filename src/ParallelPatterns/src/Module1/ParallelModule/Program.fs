// Learn more about F# at http://fsharp.org

open System
open System.Collections.Generic
open MapReduce.ParellelMapReduce

[<EntryPoint>]
let main argv =
    

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
    let output = mapReduce testInput map reduce 2 2



    0 // return an integer exit code
