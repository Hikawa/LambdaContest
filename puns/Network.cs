using System.Collections.Generic;

using System.Threading.Tasks;
using System.Net.Sockets;
using System;
using System.IO;
using System.Text;
using System.Linq.Expressions;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Puns.Network {

class DisconnectedException: Exception {
  public DisconnectedException(): base("Disconnected") {
  }
}

class Client: Game.APunter {
  public TcpClient Tcp { get; }

  public Client(TcpClient tcp) {
    Tcp = tcp;
  }

  public async Task<string> Read() {
    byte[] sizeBytes = new byte[9];
    int p = 0;
    do {
      p += await Tcp.GetStream().ReadAsync(sizeBytes, p, 1);
    } while ((p < sizeBytes.Length) && ((p == 0) || (sizeBytes [p - 1] != ':')));
    if (sizeBytes [p - 1] != ':')
      throw new Exception("Format error");
    var sizeString = new String(Encoding.ASCII.GetChars(sizeBytes), 0, p - 1);
    int size = int.Parse(sizeString);
    byte[] message = new byte[size];
    p = 0;
    do {
      int dp = await Tcp.GetStream().ReadAsync(message, p, size - p);
      if (dp == 0) throw new DisconnectedException();
      p += dp;
    } while (p < size);
    var result = new String(Encoding.ASCII.GetChars(message));
    Console.WriteLine($"Recv from {Name}/{Id}: {result}");
    return result;
  }

  public async Task Write(string message) {
    Console.WriteLine($"Send to {Name}/{Id}: {message}");
    byte[] data = Encoding.ASCII.GetBytes($"{message.Length}:{message}");
    await Tcp.GetStream().WriteAsync(data, 0, data.Length);
  }

  public async Task Handshake() {
    var request = JObject.Parse(await Read());
    Name = (string)request ["me"];
    var response = new JObject() {
      ["you" ] = Name
    };
    await Write(response.ToString(Formatting.None));
  }

  public override async Task Setup(Game.Game game) {
    var data = new JObject() { 
      ["punter"] = Id,
      ["punters"] = game.PunterCount,
      ["map"] = JObject.FromObject(game.World)
    };
    await Write(data.ToString(Formatting.None));
    var response = JObject.Parse(await Read());
    if ((int)response["ready"] != Id)
      throw new Exception($"Wrong ready id {response["ready"]} for {Id}");
  }

  public override async Task<Game.AMove> NextMove(Game.State state) {
    var data = new JObject() { 
      ["move"] = new JObject() {
        ["moves"] = JArray.FromObject(state.LastMoves)
      }
    };
    await Write(data.ToString(Formatting.None));
    //await Write(JsonConvert.SerializeObject(new MoveInfo(state)));
    return JsonConvert.DeserializeObject<Game.AMove>(await Read());
  }

  public override async Task ReportScore(Game.State state, int[] scores) {
    var scoresData = new JObject[scores.Length];
    for (int i = 0; i < scores.Length; ++i) {
      scoresData[i] = new JObject() {
        ["punter"] = i,
        ["score"] = scores[i]
      };
    };
    var data = new JObject() {
      ["stop"] = new JObject() {
        ["moves"] = JArray.FromObject(state.LastMoves),
        ["scores"] = JArray.FromObject(scoresData)
      }
    };
    await Write(data.ToString(Formatting.None));
  }
}

class Server {
  object lock_ = new object();
  List<Task> connections_ = new List<Task>(); // pending connections

  public Game.Game Game { get; private set; }

  public Server() {
  }

  public async Task AsyncListen(int listenPort, Spec.IWorld world, int punterCount) {
    var tcpListener = TcpListener.Create(listenPort);
    tcpListener.Start();
    while (true) {
      Game = new Game.Game(world, punterCount);
      while (!Game.IsFull) {
        try {
          var client = new Client(await tcpListener.AcceptTcpClientAsync());
          await client.Handshake();
          Game.AddPunter(client);
        } catch(Exception e) {
          Console.WriteLine($"Client init error: {e}");
        }
      }
      try {
        await Game.Run();
      } catch (Exception e) {
        Console.WriteLine($"Game error: {e}");
      }
    }
  }
}
}
