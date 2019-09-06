module AgentMapReduce 

    open System
    open System.IO
    open AgentEx

    let mapReduce (map    : 'T1 -> Async<'T2>)
                  (reduce : 'T2 -> 'T2 -> Async<'T2>)
                  (input  : seq<'T1>) : Async<'T2> =
    
        let run (a: Async<'T>) (k: 'T -> unit) =
            Async.StartWithContinuations(a, k, ignore, ignore)
    
        Async.FromContinuations <| fun (ok, _, _) ->
            let mutable k = 0
            let agent =
                new Agent<_>(fun chan ->
                    async {
                        for i in 2 .. k do
                            let! x = chan.Receive()
                            let! y = chan.Receive()
                            return run (reduce x y) chan.Post
                        let! r = chan.Receive()
                        return ok r
                    })
            k <- (0, input)
                 ||> Seq.fold (fun count x ->
                      run (map x) agent.Post
                      count + 1)
            agent.Start()