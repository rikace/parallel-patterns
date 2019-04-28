namespace Pipeline.FSharp

module AgentPipe = 

    type IAgent<'Input, 'Output> =
      abstract Post : 'Input -> unit
      abstract Output : IEvent<'Output>

    type Producer() =
        let outputEvent = Event<string>()

        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop state =
                    async { let! msg = inbox.Receive()
                            printfn "rec P %O" msg
                            outputEvent.Trigger (string msg)
                            return! loop (msg::state)
                    }
                loop [])
        
        interface IAgent<int, string> with
            member __.Post msg = agent.Post msg
            member __.Output = outputEvent.Publish

    type Consumer() =
        let outputEvent = Event<bool>()

        let agent =
            MailboxProcessor.Start(fun inbox ->
                let rec loop state =
                    async { let! msg = inbox.Receive()
                            printfn "rec C %O" msg
                            outputEvent.Trigger true
                            return! loop (msg::state)
                    }
                loop [])
        
        interface IAgent<string, bool> with
            member __.Post msg = agent.Post msg
            member __.Output = outputEvent.Publish

    let pipe (producer: IAgent<_, _>) (consumer: IAgent<_, _>) =
      producer.Output.Add consumer.Post            

    let p = Producer()
    let c = Consumer()

    let run = pipe p c      

    (p :> IAgent<_,_>).Post(1)