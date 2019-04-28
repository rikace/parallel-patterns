
open System
open System.IO
open System.Net
open Microsoft.FSharp.Control
open System.Threading
open System.Threading.Tasks
open System.Collections.Generic

type Agent<'T> = MailboxProcessor<'T>

let start<'T> (work : 'T -> unit) =
    Agent<obj>.Start(fun mb ->
        let rec loop () =
            async {
                let! msg = mb.Receive()
                match msg with
                | :? 'T as msg' -> work msg'
                | _ -> () // oops... undefined behaviour
                return! loop ()
            }
        loop () )



let counter =
    new Agent<_>(fun inbox ->
        let rec loop n =
            async {printfn "n = %d, waiting..." n
                   let! msg = inbox.Receive()
                   return! loop (n + msg)}
        loop 0)

counter.Start()
counter.Post(1)
counter.Post(2)
counter.Post(1)


let agent =
    Agent<string>.Start(fun inbox ->
        let rec loop count = async {
            let! message = inbox.Receive()
            use client = new WebClient()
            let uri = Uri message
            let! site = client.AsyncDownloadString(uri)
            printfn "Size of %s is %d - total messages %d" uri.Host site.Length (count + 1)
            return! loop (count + 1) }
        loop 0)

agent.Post "http://www.google.com"
agent.Post "http://www.microsoft.com"


// create agents
let agents =
    [1 .. 100 * 1000]
    |> List.map  (fun i->
       Agent.Start(fun n ->
         async {
            while true do
               let! msg = n.Receive()
               if i % 20000 = 0 then
                   printfn "agent %d got message '%s'" i msg } ))

#time "on"
// post message to all agents
for agent in agents do
    agent.Post "hello"



/// The internal type of messages for the agent
type internal msg = Increment of int | Fetch of AsyncReplyChannel<int> | Stop

type CountingAgent() =
    let counter = MailboxProcessor.Start(fun inbox ->
         // The states of the message-processing state machine...
         let rec loop n =
             async {let! msg = inbox.Receive()
                    match msg with
                    | Increment m ->
                        // increment and continue...
                        return! loop(n + m)
                    | Stop ->
                        // exit
                        return ()
                    | Fetch replyChannel  ->
                        // post response to reply channel and continue
                        do replyChannel.Reply n
                        return! loop n}

         // The initial state of the message-processing state machine...
         loop(0))

    member a.Increment(n) = counter.Post(Increment n)
    member a.Stop() = counter.Post Stop
    member a.Fetch() = counter.PostAndReply(fun replyChannel -> Fetch replyChannel)

let countingAgent = new CountingAgent()
countingAgent.Increment(1)
countingAgent.Fetch()
countingAgent.Increment(2)
countingAgent.Fetch()
countingAgent.Stop()



(******************************* AGENT ERROR HANDLING ***********************************)
let errorAgent =
       Agent<int * System.Exception>.Start(fun inbox ->
         async { while true do
                   let! (agentId, err) = inbox.Receive()
                   printfn "an error '%s' occurred in agent %d" err.Message agentId })


let agents10000 =
       [ for agentId in 0 .. 10000 ->
            let agent =
                new Agent<string>(fun inbox ->
                   async { while true do
                             let! msg = inbox.Receive()
                             if msg.Contains("agent 99") then
                                 failwith "fail!" })
            // Error Handling
            agent.Error.Add(fun error -> errorAgent.Post (agentId,error))
            agent.Start()
            (agentId, agent) ]

for (agentId, agent) in agents10000 do
    agent.Post (sprintf "message to agent %d" agentId )




module Agent =
    let reportErrorsTo (supervisor: Agent<exn>) (agent: Agent<_>) =
           agent.Error.Add(fun error -> supervisor.Post error); agent

    let startAgent (agent: Agent<_>) = agent.Start(); agent

let supervisor' =
   Agent<System.Exception>.Start(fun inbox ->
     async { while true do
               let!err = inbox.Receive()
               printfn "an error '%s' occurred" err.Message })

let agent' =
   new Agent<int>(fun inbox ->
     async { while true do
               let! msg = inbox.Receive()
               if msg % 1000 = 0 then
                   failwith "I don't like that cookie!" })
   |> Agent.reportErrorsTo supervisor'
   |> Agent.startAgent

for i in [0..1000] do
    agent'.Post i


// ===========================================
// Agent perf message-sec
// ===========================================

let [<Literal>] count = 2000000

let agentPerf() =
    Agent<int>.Start(fun inbox ->
            let sw = System.Diagnostics.Stopwatch()
            let rec loop() = async{
                let! msg = inbox.Receive()
                if msg = 0 then
                    sw.Start()
                    return! loop()
                elif msg = count then
                    printfn "Last message arrived - %d ms - %d message per sec" sw.ElapsedMilliseconds (count/  sw.Elapsed.Seconds)
                else
                    return! loop() }
            loop())

let sw = System.Diagnostics.Stopwatch.StartNew()
let agentsPerf = Array.init count (fun _ -> agentPerf())
printfn "Time to create %d Agents - %d ms" count sw.ElapsedMilliseconds

sw.Restart()
agentsPerf |> Array.iteri(fun i a -> a.Post(i))

printfn "Last message sent - %d ms - %d message per sec" sw.ElapsedMilliseconds (count/  sw.Elapsed.Seconds)
