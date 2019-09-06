module RequestGate
open System.Threading

type RequestGate(n:int) =
    let semaphore = new System.Threading.SemaphoreSlim(n,n)
    member x.Aquire(?timeout) =
        async {
            
            // TODO 
            // implement the logic to coordinate the access to resources 
            // using "semaphore". Keep async semantic for the "acquire" and "release" of the handle
            // throw new Exception("No implemented");
            
            return! failwith "Handle not aquired"
        }


