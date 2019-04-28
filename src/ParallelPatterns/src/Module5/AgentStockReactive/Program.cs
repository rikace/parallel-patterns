using ReactiveStock.ActorModel.Actors;
using ReactiveStock.ActorModel.Actors.UI;
using ReactiveStock.ActorModel.Messages;
using System;
using System.Collections.Generic;

namespace AgentStockReactive
{
    class Program
    {
        static void Main(string[] args)
        {
            var chartAgent = new LineChartingActor();
            var stockCoordinator = new StocksCoordinatorActor(chartAgent.Actor);

            var stocks = new List<(string, ConsoleColor)>();
            stocks.Add(("MSFT", ConsoleColor.Red));
            stocks.Add(("APPL", ConsoleColor.Cyan));
            stocks.Add(("GOOG", ConsoleColor.Green));

            foreach (var item in stocks)
            {
                stockCoordinator.Actor.Post(new WatchStockMessage(item.Item1, item.Item2));
            }
            
           Console.ReadLine();
        }
    }
}
