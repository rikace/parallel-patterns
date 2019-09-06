﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Combinators
{
    public static class TaskCombinators
    {
        

        public static async Task CombinatorRedundancy()
        {
            // Redundancy with Task.WhenAny
            var cts = new CancellationTokenSource();

            Func<string, string, string, CancellationToken, Task<string>> GetBestFlightAsync =
                async (from, to, carrier, token) =>
                {
                    string url = $"flight provider {carrier}";
                    using (var client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url, token);
                        return await response.Content.ReadAsStringAsync();
                    }
                };

            var recommendationFlights = new List<Task<string>>()
            {
                GetBestFlightAsync("WAS", "SF", "United", cts.Token),
                GetBestFlightAsync("WAS", "SF", "Delta", cts.Token),
                GetBestFlightAsync("WAS", "SF", "AirFrance", cts.Token),
            };

            Task<string> recommendationFlight = await Task.WhenAny(recommendationFlights);
            while (recommendationFlights.Count > 0)
            {
                try
                {
                    var recommendedFlight = await recommendationFlight;
                    cts.Cancel();
                    BuyFlightTicket("WAS", "SF", recommendedFlight);
                    break;
                }
                catch (WebException)
                {
                    recommendationFlights.Remove(recommendationFlight);
                }
            }
        }

        private static void BuyFlightTicket(string v1, string v2, string recommendedFlight)
            => new NotImplementedException(); // implementation for buying the tickets

        // Asynchronous For-Each loop with Task.WhenAll
        static Task ForEachAsync<T>(this IEnumerable<T> source, int maxDegreeOfParallelism, Func<T, Task> body)
        {
            return Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(maxDegreeOfParallelism)
                select Task.Run(async () =>
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current);
                }));
        }


        // Using the asynchronous For Each loop
        static async Task SendEmailsAsync(List<string> emails)
        {
            SmtpClient client = new SmtpClient();

            Func<string, Task> sendEmailAsync = async emailTo =>
            {
                MailMessage message = new MailMessage("me@me.com", emailTo);
                await client.SendMailAsync(message);
            };

            await emails.ForEachAsync(Environment.ProcessorCount, sendEmailAsync);
        }
    }
}