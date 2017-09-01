using System;
using System.IO;

using Newtonsoft.Json;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Puns {
class MainClass {
  public static void Main(string[] args) {
    var mapFileName = "/home/aankor/Dropbox/icfpc17/oxford.json";
    if (args.Length > 0)
      mapFileName = args [0];
    var listenPort = 8000;
    if (args.Length > 1)
      listenPort = int.Parse(args [1]);
    var punterCount = 2;
    if (args.Length > 2)
      punterCount = int.Parse(args [2]);
    var s = File.ReadAllText(mapFileName);
    Spec.IWorld world = JsonConvert.DeserializeObject<Spec.World>(s);
    new Network.Server().AsyncListen(listenPort, world, punterCount).Wait();
  }
}
}
