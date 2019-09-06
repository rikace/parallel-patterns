namespace ParallelPatterns.FSharp.Common

module Async = 
    let retn x = async { return x }
  
    let apply (funAsync:Async<'a -> 'b>) (opAsync:Async<'a>) = async {
        let! funAsyncChild = Async.StartChild funAsync
        let! opAsyncChild = Async.StartChild opAsync
    
        let! funAsyncRes = funAsyncChild
        let! opAsyncRes = opAsyncChild
        return funAsyncRes opAsyncRes
        }
 
 
    let (<*>) = apply
    let map f x = retn f <*> x
    
    let (<!>) = map
    
    let lift2 f x y = 
        f <!> x <*> y
        
    let lift3 f x y z = 
        f <!> x <*> y <*> z
        
    let lift4 f x y z w = 
        f <!> x <*> y <*> z <*> w
        
    let traverse f list =
        let folder x xs = retn (fun x xs -> x :: xs) <*> f x <*> xs        
        List.foldBack folder list (retn [])
        
