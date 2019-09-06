using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Akka.Actor;

using AkkaFractal.Core;
using AkkaFractal.Web.Akka;

using Microsoft.AspNetCore.Mvc;
using AkkaFractal.Web.Models;

using Lib.AspNetCore.ServerSentEvents;

namespace AkkaFractal.Web.Controllers
{
    public class HomeController : Controller
    {
        public HomeController(SseTileActorProvider actorProvider)
        {
            _sseTileActorRef = actorProvider();
        }

        private readonly IActorRef _sseTileActorRef;

        [HttpGet("run")]
        public void Run()
        {
            _sseTileActorRef.Tell(new RenderImage(4000,4000));
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
