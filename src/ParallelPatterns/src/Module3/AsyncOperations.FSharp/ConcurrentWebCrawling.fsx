module ConcurrentWebCrawling =
    open System.Collections.Generic
    open System.Net
    open System.IO
    open System.Threading
    open System.Text.RegularExpressions

   //  Our web crawler must also be able to handle relative addresses. 
   //  Extracting links using regular expressions
   //  Let us consider the references to other URIs that appear in the href attribute of tags in web pages: 
   //  <a href="...">
   //  The following regular expression achieves this:
   //  href="([^"]+)
   //      for x in link.Matches html ->
   //          x.Groups.[1].Value ]
   //  
   //  The Matches method returns the sequence of substrings in html that were found to match the link regular expression. 
   //  The actual substring matched is of the form href="http://... and the text of the URI itself is given at 
   //  the index 1 of the Groups because the regular expression bracketed this subexpression. 
   //  The Value property extracts the string itself which may now be used to construct a System.Uri object. *)

   // Non-concurrent web crawler
   // In the context of web crawling, concurrency is an optimization.
   // Therefore, let us try to build a simple non-concurrent web crawler first.

   // The web crawler maintains a set of URIs that have already been visited. 
    type ComparableUri(uri: System.Uri) =
        inherit System.Uri(uri.AbsoluteUri)
    
        let elts (uri: System.Uri) =
          uri.Scheme, uri.Host, uri.Port, uri.Segments
    
        interface System.IComparable with
          member this.CompareTo(uri2) =
            compare (elts this) (elts(uri2 :?> ComparableUri))
    
        override this.Equals(uri2) =
          compare this (uri2 :?> ComparableUri) = 0

    // We instantiate the regular expression that will be used to extract URIs from HTML:
    let link =
        let opts = System.Text.RegularExpressions.RegexOptions.Compiled
        System.Text.RegularExpressions.Regex("href=\"([^\"]+)", opts)

    // Note that we have enabled compilation of the regular expression to optimized native code.
    // The following download function fetches the HTML from the given URI: 
    let download (url: System.Uri) =
        let client = new System.Net.WebClient()
        let enc = System.Text.UTF8Encoding()
        client.DownloadData url |> enc.GetString

    // The following extract function uses the regular expression link to identify URIs referenced from within the given html : 
    let extract baseUri html =
        [ for url in link.Matches html ->
            try [ComparableUri(System.Uri(baseUri, url.Groups.[1].Value))] with _ -> [] ]
        |> List.concat

    // The non-concurrent crawler is always in one of two states:
    type state =
        | Crawl of System.Uri
        | Finished

    // The crawler uses two pieces of global mutable state. The first is the set of visited URIs. 
    // The second is the queue of URIs waiting to be visited, represented using the mutable Queue implementation from the .NET framework. 
    // This mutable state can be productively encapsulated as private data within a UriCollector class 
    // in order to minimize the area of code that can mutate these data structures
    type UriCollector(homeUri: System.Uri) =
        let mutable visited : Set<ComparableUri> = Set.empty
        let waiting : System.Collections.Generic.Queue<ComparableUri> =
          System.Collections.Generic.Queue()
        let invalid (homeUri : System.Uri) uri =
          visited.Contains uri || not(homeUri.IsBaseOf uri)
        do
          waiting.Enqueue(ComparableUri homeUri)
        member this.Pop() =
          if waiting.Count = 0 then Finished else
            let uri = waiting.Dequeue()
            if invalid homeUri uri then this.Pop() else
              visited <- visited.Add uri
              printf "%d: %s\n" waiting.Count uri.AbsoluteUri
              Crawl uri
        member this.Push uris =
          Seq.iter waiting.Enqueue uris
        member this.Visited = visited

    // The Pop member function determines the current state of the crawler by trying to find a valid 
    // URI to crawl or returning Finished otherwise. The Push member function 
    // pushes the given links onto the queue of URIs waiting to be visited. 
    // Finally, the following crawl function is the core of the web crawler: 
        member this.Crawl() =
            let rec crawl() =
                match this.Pop() with
                | Finished -> ()
                | Crawl url ->
                    let refs = try download url |> extract url with _ -> []
                    this.Push refs
                    crawl()
            crawl()

    // This function attempts to pop a URI from the collection of URIs waiting to be crawled, 
    // crawls it (if available) and pushes the URIs referenced from it onto the waiting list before recursing.
    
    let crawl uri =
        let collector = UriCollector uri
        let t = System.Diagnostics.Stopwatch.StartNew()
        collector.Crawl()
        printf "Visited %d URLs in %dms\n" collector.Visited.Count t.ElapsedMilliseconds

    // System.Uri("http://www.microsoft.com") |> crawl
    System.Uri("http://www.google.com") |> crawl



    // Concurrent implementation
    // The conventional and lowest-level approach to concurrent programming is based upon multithreading
    // because IO operations like downloading a web page only block the current thread and other threads may continue to execute. 
    // An alternative is message passing and F#'s asynchronous workflows are designed to facilitate this. 
    // This section describes a similar web crawler that uses asynchronous workflows to fetch data concurrently. 
    // This amortizes the latency of fetching data and greatly improves performance when latency is a bottleneck.

    // The following crawl function fetches HTML from the given URI, extracts the links and calls the post
    // function to crawl each link before calling the post function with Finished to indicate that 
    // the download of this URI has been completed: 
    
    // (state -> unit) -> ComparableUri -> Async<unit>
    let crawlConcurrent post (uri: ComparableUri) =
        async { try
                  let! html =
                    async { let req = System.Net.WebRequest.Create(uri, Timeout=5)
                            use! response = req.AsyncGetResponse() //|> Async.AwaitTask
                            use reader = new System.IO.StreamReader(response.GetResponseStream())
                            return reader.ReadToEnd() }
                  for link in extract uri html do
                    post(Crawl link)
                finally post Finished }

    // Note how the final post of Finished appears after the try..finally construct to ensure that it is always posted. 
    // The following uriCollector function creates a mailbox that repeatedly handles messages until the web crawler completes: 
    let uriCollector (baseUri: System.Uri) =
        use waitHandle = new System.Threading.AutoResetEvent(false)
        let result = ref Set.empty
        let mailBox =
          MailboxProcessor.Start(fun self ->
            let rec waitForUrl (visited: Set<ComparableUri>, downloads) =
              async { let! message = self.Receive()
                      match message with
                      | Crawl uri ->
                          let uri = ComparableUri uri
                          if not (visited.Contains uri) && baseUri.IsBaseOf uri then
                            let! tempAsync = Async.StartChild(crawlConcurrent self.Post uri)
                            do! tempAsync
                            return! waitForUrl(visited.Add uri, downloads + 1)
                          else
                            return! waitForUrl(visited, downloads)
                      | Finished ->
                          if downloads > 1 then
                            return! waitForUrl(visited, downloads - 1)
                          else
                            printf "Finished\n"
                            result := visited
                            waitHandle.Set() |> ignore
                            return () }
            waitForUrl(Set.empty, 0))
        mailBox.Post(Crawl(ComparableUri baseUri))
        waitHandle.WaitOne() |> ignore
        !result

    let t = System.Diagnostics.Stopwatch.StartNew()
    let visited = uriCollector(System.Uri "http://www.microdoft.com")
    printf "Visited %d URIs in %dms\n" visited.Count t.ElapsedMilliseconds
    
