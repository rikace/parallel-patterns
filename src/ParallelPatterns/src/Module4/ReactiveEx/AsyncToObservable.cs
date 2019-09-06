using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;

namespace ReactiveEx
{
    class AsyncToObservable
    {
        static IObservable<string> FetchWebpage(string url)
        {
            var hwr = WebRequest.CreateDefault(new Uri(url)) as HttpWebRequest;
            var requestFunc = hwr.GetResponseAsync();

            // TODO 
            // Generate a WebResponse Observable
            // then, using the web-response, extract the text of the
            // http content from the underlying stream

            return Observable.Empty<string>();
        }
        
        public static void Start()
        {
            var inputs = (new[] {
                    "http://www.google.com",
                    "http://www.duckduckgo.com",
                    "http://www.yahoo.com",
                    "http://www.bing.com",
            });

            inputs.ToObservable()
            .SelectMany(x => Observable.Defer(() =>
            {
                Console.WriteLine("Requesting page for " + x);
                return FetchWebpage(x);
            })
                .Timeout(TimeSpan.FromMilliseconds(750))
                .Retry(3)
                .Catch(Observable.Return(x + " Error")))
                //.OnErrorResumeNext(Observable.Return("Couldn't fetch the Website")))
                .Subscribe(x => Console.WriteLine($"Sub {x.Substring(0, 10)} - Thread {Thread.CurrentThread.ManagedThreadId}"));
        }
    }
}
