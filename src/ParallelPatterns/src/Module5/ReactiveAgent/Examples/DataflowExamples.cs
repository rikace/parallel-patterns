﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ReactiveAgent.CS
{

    class DataflowTransformActionBlocks
    {
        public static void Run()
        {
            //  Download image using TPL Dataflow TransformBlock
            var fetchImageFlag = new TransformBlock<string, (string, byte[])>(
                async urlImage =>
                {
                    using (var webClient = new WebClient())
                    {
                        byte[] data = await webClient.DownloadDataTaskAsync(urlImage);
                        return (urlImage, data);
                    }
                });

            //  Persist data using TPL Dataflow ActionBlock
            var saveData = new ActionBlock<(string, byte[])>(async data =>
            {
                (string urlImage, byte[] image) = data;
                string filePath = urlImage.Substring(urlImage.IndexOf("File:") + 5);
                await Agents.File.WriteAllBytesAsync(filePath, image);
            });

            fetchImageFlag.LinkTo(saveData);
            
            List<string> urlFlags = new List<string>{
                "Italy#/media/File:Flag_of_Italy.svg",
                "Spain#/media/File:Flag_of_Spain.svg",
                "United_States#/media/File:Flag_of_the_United_States.svg"
            };

            foreach (var urlFlag in urlFlags)
                fetchImageFlag.Post($"https://en.wikipedia.org/wiki/{urlFlag}");

            
        }
    }
}