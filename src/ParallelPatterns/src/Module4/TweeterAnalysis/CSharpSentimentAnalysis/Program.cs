using System;
using System.IO;
using System.Linq;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi;
using Tweetinvi.Streaming.Parameters;
using Rx = System.Reactive.Linq;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Threading.Tasks;
using SentimentAnalysis = FSharpSentimentAnalysis.SentimentAnalysis;

namespace CSharpSentimentAnalysis
{
    class RxTweetEmotion : RxPublisherSubscriber<ITweet>
    {
        public RxTweetEmotion(TimeSpan throttle)
        {
            var obs =
                DataProducer.TweetStream.GetReactiveTweets(throttle)
                    .SubscribeOn(TaskPoolScheduler.Default);
            base.AddPublisher(obs);
        }
    }

    class Program
    {
        static void Print(string text, ConsoleColor color)
        {
            var bakColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = bakColor;
        }

        static void Main(string[] args)
        {
            var sentimentModel = Analysis.ML.loadModel("../../../Data/SentimentAnalysis/model.zip");

            // TODO 
            // add weather service Shared.Reactive.Utils.GetWeatherAsync information 
            // retrieve weather info base on the tweet coordinates
            var tweetPositiveObserver = Observer.Create<ITweet>(tweet =>
            {
                var sentimentPrediction = Analysis.ML.runPrediction(sentimentModel, tweet.Text);
                var scoreSentiment = Analysis.ML.scorePrediction(sentimentPrediction);
                var scoreNormalized = SentimentAnalysis.normalize(scoreSentiment);
                var sentiment = new SentimentAnalysis.EmotionType(scoreNormalized);

                Print($"{sentiment.ToString()} - {tweet.Text}", SentimentAnalysis.EmotionType.consoleColor(sentiment));
            });

                     
            // Using Rx
            var obs = DataProducer.TweetStream.GetReactiveTweets(TimeSpan.FromMilliseconds(150))
                .SubscribeOn(TaskPoolScheduler.Default);
            obs.Subscribe(tweetPositiveObserver);

            // TODO
            // replace the previous code with the RxPublisherSubscriber object,
            // then create few more (2 / 3) different observers to subscribe  
            // complete the "AddPublisher"  in the RxPublisherSubscriber class
            
            // TODO
            // elaborate the Observable output
            // Ex group events by sentiment, print output every 10 events... 
            
            Console.WriteLine("Press `Enter` to exit.");
            Console.WriteLine("======================");
            Console.ReadLine();
        }


        static IObservable<string> TweetEmotionObservable(TimeSpan throttle)
        {
            var consumerKey = "<your Key>";
            var consumerSecretKey = "<your secret key>";
            var accessToken = "<your access token>";
            var accessTokenSecret = "<your secret access token>";

            return
                Rx.Observable.Create<string>(observer =>
                {
                    var cred = new TwitterCredentials(
                        consumerKey, consumerSecretKey, accessToken, accessTokenSecret);

                    var stream = Tweetinvi.Stream.CreateSampleStream(cred);
                    stream.FilterLevel = StreamFilterLevel.Low;
                    stream.StartStreamAsync();

                    return
                        Rx.Observable
                            .FromEventPattern<TweetReceivedEventArgs>(stream, "TweetReceived")
                            .Throttle(throttle)
                            .Select(args => args.EventArgs)
                            // .Where(args => args.Tweet.Language == Language.English)
                            // .GroupBy(args =>  SentimentAnalysis
                            .Subscribe(o => observer.OnNext(o.Tweet.FullText));
                });
        }
    }
}