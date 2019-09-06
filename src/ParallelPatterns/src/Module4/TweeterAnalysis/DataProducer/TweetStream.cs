using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Shared.Reactive.Tweets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using Tweetinvi.Models;

namespace DataProducer
{
    public static class TweetStream
    {
        public static IObservable<ITweet> GetReactiveTweets(TimeSpan throttle,
            string filePath = @"../../../Data/Tweets/tweets.txt")
        {
            var tweetFilePath = filePath;
            return Observable.Using(
                () => new StreamReader(tweetFilePath),
                reader => Observable.FromAsync(reader.ReadLineAsync)
                    .Throttle(throttle)
                    .TakeWhile(line => line != null)
                    .Repeat()
                    .Select(line =>
                    {
                        var json = JObject.Parse(line);
                        return new Tweet(json["TweetDTO"]);
                    }));
        }
    }
}
