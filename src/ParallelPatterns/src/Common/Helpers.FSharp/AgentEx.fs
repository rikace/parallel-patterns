namespace AgentEx

[<AutoOpen>]
module AgentHelpers =

    type Agent<'a> = MailboxProcessor<'a>    
    
    let inline (<--) (agent:Agent<_>) msg = agent.Post msg      
    let inline (<-!) (agent:Agent<_>) msg = agent.PostAndAsyncReply msg     
    
    [<RequireQualifiedAccess>]
    module Agent =

        let cancelWith cancellationToken body = new Agent<_> (body,cancellationToken)
                    
        let reportErrorsTo (supervisor: Agent<exn>) (agent: Agent<_>) =
           agent.Error.Add(fun error -> supervisor.Post error); agent            
                
        let start (agent:Agent<_>) = agent.Start (); agent
        
        let supervisor f =
           Agent<System.Exception>.Start(fun inbox ->
             async { while true do
                       let! err = inbox.Receive()
                       f err })        
