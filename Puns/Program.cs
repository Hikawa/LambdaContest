using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Puns {
  internal static class Program {

    public static void Main(string[] args) {
      var mapFileName = "/home/aankor/Dropbox/icfpc17/lambda.json";
      if (args.Length > 0)
        mapFileName = args [0];
      var listenPort = 8000;
      if (args.Length > 1)
        listenPort = int.Parse(args [1]);
      var punterCount = 2;
      if (args.Length > 2)
        punterCount = int.Parse(args [2]);
      var s = File.ReadAllText(mapFileName);
      var world = JsonConvert.DeserializeObject<Punter.Spec.World>(s);
      var server = new Punter.Network.Server();
      while (true) {
        try { server.RunGame(listenPort, world, punterCount).Wait(); }
        catch (AggregateException e) {
          System.Console.WriteLine($"Game error: {e.InnerExceptions.First()}");
        }
        System.Console.WriteLine("Finished");
      }        
    }
  }
}
