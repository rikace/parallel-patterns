namespace WebCrawler

// TODO : 4.5

// Agent can accept two types of messages - one is to enqueue
// more work items (and mark a URL as visited) and the other
// is to get the next work item
type private CrawlerMessage =
  | EnqueueWorkItems of (string * string[])
  // TODO add reply back channel to caller with the pending item
  | GetWorkItem

// TODO
// (1) add the state to the mailbox
// to keep track of the visited links

/// The agent can be used to store state of crawling. The caller can use
/// 'GetAsync' method to get the next work item from the queue and it can
/// report result using 'Enqueue' (this will mark URL as visited and add
/// more URLs to the working queue).
type CrawlerAgent() =
  let agent = MailboxProcessor.Start(fun agent ->

    /// Add processed URL to the 'visited' set and then add all
    /// new pending URLs and filter those that were visited
    let rec addItems pending (from, work) =
      let newWork =
        List.append pending (List.ofSeq work)
        |> List.filter (fun v -> true)
            // Suggestion replace the (fun v -> true)
            // with functionality to check already visited links
            // to avoid non necessary re-computation
      nonEmpty newWork

    /// Represents state when the pending queue contains some URLs
    and nonEmpty pending = async {
      let! msg = agent.Receive()
      match msg with
      | EnqueueWorkItems(res) ->
          // We can add more items to the queue
          return! addItems pending res
      | GetWorkItem ->
          // There are some pending items, so we can send one back
          match pending with
          | first::rest ->
              if rest = [] then return! empty
              else return! nonEmpty rest
          | _ -> failwith "unexpected" }

    /// Represents state when the pending queue is emtpy
    and empty =
      agent.Scan(function
        // We can add more items to the queue
        | EnqueueWorkItems(res) ->
            Some(addItems [] res)
        // We cannot process 'GetWorkItem' message in this state
        | _ -> None)

    empty)  

  /// Start the agent by adding the specified URL to work items
  member x.Start(url) =
    agent.Post(EnqueueWorkItems("n/a", [| url |]))
  /// Enqueue results of crawling and mark 'from' url as visited
  member x.Enqueue(from, urls) =
    agent.Post(EnqueueWorkItems(from, urls))
  /// Asynchronously get the next work item (returns Task that can be used from C#)

    // TODO
    // complete, use the PostAndAsyncReply, for example
  member x.GetAsync() = ()
    // agent.PostAndAsyncReply
    // keep interporability with C# (Use Task)


module AgentWebCrawler =
    let urls = ["https://edition.cnn.com"; "http://www.bbc.com"; "https://www.microsoft.com" ]
    
    let start () =
        let crawler = CrawlerAgent()
        for url in urls do crawler.Start(url)
