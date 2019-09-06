﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;
using Akka.Actor;
using Newtonsoft.Json;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Stream = Tweetinvi.Stream;
using System.Net.Http;
using Tweetinvi.Core.Extensions;

namespace Shared.Reactive
{
    public static class Utils
    {
        public static void StartSampleTweetStream(IActorRef actor, LanguageFilter langFilter = LanguageFilter.English)
        {
            var stream = Stream.CreateSampleStream();
            stream.AddTweetLanguageFilter(langFilter);

            stream.TweetReceived += (_, arg) =>
            {
                arg.Tweet.Text = arg.Tweet.Text.Replace("\r", " ").Replace("\n", " ");

                if (arg.Tweet.Coordinates != null)
                    SaveTweet(arg.Tweet);

                actor.Tell(arg.Tweet);
            };
            stream.StartStream();
        }

        public static void StartFilteredTweetStream(IActorRef actor, string track, LanguageFilter langFilter = LanguageFilter.English)
        {
            var stream = Stream.CreateFilteredStream();

            stream.AddTrack(track);
            stream.AddTweetLanguageFilter(langFilter);

            stream.MatchingTweetReceived += (_, arg) =>
            {
                arg.Tweet.Text = arg.Tweet.Text.Replace("\r", " ").Replace("\n", " ");
                SaveTweet(arg.Tweet);
                actor.Tell(arg.Tweet);
            };
            stream.StartStreamMatchingAnyCondition();
        }

        private static void SaveTweet(ITweet tweet)
        {
            var json = JsonConvert.SerializeObject(tweet);
            File.AppendAllText("../../../Tweets/tweets.txt", $"{json}\r\n");
        }

        public static async Task<decimal> GetHttpWeatherAsync(ICoordinates coordinates)
        {
            using (var httpClient = new HttpClient())
            {
                var requestUrl = $"http://api.met.no/weatherapi/locationforecast/1.9/?lat={coordinates.Latitude};lon={coordinates.Latitude}";
                var result = await httpClient.GetStringAsync(requestUrl);
                var doc = XDocument.Parse(result);
                var temp = doc.Root.Descendants("temperature").First().Attribute("value").Value;
                return decimal.Parse(temp);
            }
        }

        public static Func<T, Task<R>> Cache<T, R>(Func<T, Task<R>> factory)
        {
            var cache = new ConcurrentDictionary<T, Lazy<Task<R>>>();
            return key => cache.GetOrAdd(key, k => new Lazy<Task<R>>(() => factory(k))).Value;
        }


        public static async Task<decimal> GetWeatherAsync(ICoordinates coordinate)
        {
            using (var stream = File.OpenRead(@"../../../Data/Tweets/weather.txt"))
            using (var reader = new StreamReader(stream))
            {
                string json = await reader.ReadToEndAsync();

                var tweetCoordinates =
                    JsonConvert.DeserializeObject<List<TweetCoordinates>>(json);

                var temp = from coord in tweetCoordinates
                           where Math.Abs(coord.Longitude - coordinate.Longitude) < 0.004
                                 && Math.Abs(coord.Latitude - coordinate.Latitude) < 0.004
                           select coord.Temp;

                await Task.Delay(200);

                if (temp.IsEmpty())
                    return new Random().Next(8, 40);
                return temp.First();
            }
        }

        public static string FormatTweet(ITweet tweet)
        {
            var builder = new StringBuilder();
            builder.AppendLine("---------------------------------------------------------");
            builder.AppendLine($"Tweet from {tweet.CreatedBy} at {tweet.Coordinates?.Latitude},{tweet.Coordinates?.Longitude}");
            builder.AppendLine(tweet.Text);
            return builder.ToString();
        }

        public static string FormatUser(IUser user)
        {
            return user.ToString();
        }

        public static string FormatCoordinates(ICoordinates coordinates)
        {
            return $"------------------------------------{coordinates?.Latitude},{coordinates?.Longitude}";
        }

        public static string FormatTemperature(decimal temperature)
        {
            return $"------------------------------------{temperature}° Celcius";
        }

        public static string FormatLocation((ICoordinates, string) arg)
        {
            return $"Coordinates: Lat {arg.Item1.Latitude}\t\t Lon {arg.Item1.Longitude}\n------------------------ Location: {arg.Item2}";
        }
    }
}
