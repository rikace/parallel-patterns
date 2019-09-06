module AgentWebCrawler.Common

open System.Threading
open System.IO
open System.Net
open System.Text.RegularExpressions

type Agent<'a> = MailboxProcessor<'a>

type Msg<'a, 'b> =
| Item of 'a
| Mailbox of Agent<Msg<'a, 'b>>
    
let httpRgx =
    new ThreadLocal<Regex>(fun () -> new Regex(@"^(http|https|www)://.*$"))
    
let printerAgent (token : CancellationToken) = 
    Agent.Start((fun inbox -> async {
      while true do 
        let! msg = inbox.Receive()                
        match msg with
        | Item(t) -> printfn "%s" t
        | Mailbox(agent) -> failwith "no implemented"}), cancellationToken = token)
    
        
let downloadContent (url : string) = async {
    try
      let req = WebRequest.Create(url) :?> HttpWebRequest
      req.UserAgent <- "Mozilla/5.0 (Windows; U; MSIE 9.0; Windows NT 9.0; en-US)"
      req.Timeout <- 5000
      use! resp = req.GetResponseAsync() |> Async.AwaitTask
      let content = resp.ContentType
      let isHtml = Regex("html").IsMatch(content)
      match isHtml with
      | true -> use stream = resp.GetResponseStream()
                use reader = new StreamReader(stream)
                let! html = reader.ReadToEndAsync() |> Async.AwaitTask
                return Some html
      | false -> return None
    with
    | _ -> return None
}
