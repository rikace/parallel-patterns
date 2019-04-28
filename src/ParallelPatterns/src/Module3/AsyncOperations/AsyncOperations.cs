using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Functional.Async;

namespace AsyncOperations
{
    public class AsyncOperations
    {
        void ReadFileBlocking(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                          FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead = fileStream.Read(buffer, 0, buffer.Length);
                process(buffer);
            }
        }

        // Read from the file system asynchronously
        IAsyncResult ReadFileNoBlocking(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                    FileAccess.Read, FileShare.Read, 0x1000,
                                                     FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[fileStream.Length];
                var state = Tuple.Create(buffer, fileStream, process);
                return fileStream.BeginRead(buffer, 0, buffer.Length,
                                              EndReadCallback, state);
            }
        }

        void EndReadCallback(IAsyncResult ar)
        {
            var state = ar.AsyncState as Tuple<byte[], FileStream, Action<byte[]>>;
            using (state.Item2) state.Item2.EndRead(ar);
            state.Item3(state.Item1);
        }


        async void ReadFileNoBlockingAsync(string filePath, Action<byte[]> process)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Open,
                                            FileAccess.Read, FileShare.Read, 0x1000,
                                            FileOptions.Asynchronous))
            {
                byte[] buffer = new byte[fileStream.Length];
                int bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length);
                await Task.Run(async () => process(buffer));
            }
        }

        private static void CancelTask()
        {
            //  Cancellation Token callback
            CancellationTokenSource tokenSource = new CancellationTokenSource();
            CancellationToken token = tokenSource.Token;

            Task.Run(async () =>
            {
                var webClient = new WebClient();
                token.Register(() => webClient.CancelAsync());

                var data = await
                    webClient.DownloadDataTaskAsync("http://www.manning.com");
            }, token);

            tokenSource.Cancel();
        }
        void CooperativeCancellation()
        {
            //  Cooperative cancellation token
            CancellationTokenSource ctsOne = new CancellationTokenSource();
            CancellationTokenSource ctsTwo = new CancellationTokenSource();
            CancellationTokenSource ctsComposite = CancellationTokenSource.CreateLinkedTokenSource(ctsOne.Token, ctsTwo.Token);

            CancellationToken ctsCompositeToken = ctsComposite.Token;
            Task.Factory.StartNew(async () =>
            {
                var webClient = new WebClient();
                ctsCompositeToken.Register(() => webClient.CancelAsync());

                await webClient.DownloadDataTaskAsync("http://www.manning.com");
            }, ctsComposite.Token);
        }

    }
}
