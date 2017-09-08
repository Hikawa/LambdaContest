using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Punter.Network {
  public class Client: Game.APunter {
    public TcpClient Tcp { private get; set; }

    public Client(int id)
      : base(id) {}
    
    private async Task<string> Read(CancellationToken ct) {
      var sizeBytes = new byte[9];
      var p = 0;
      do p += await Tcp.GetStream().ReadAsync(sizeBytes, p, 1, ct);
      while ((p < sizeBytes.Length)
             && ((p == 0) || (sizeBytes[p - 1] != ':')));
      if (sizeBytes[p - 1] != ':')
        throw new Exception("Format error");
      var sizeString = new string(Encoding.ASCII.GetChars(sizeBytes), 0, p - 1);
      var size = int.Parse(sizeString);
      var message = new byte[size];
      p = 0;
      do {
        var dp = await Tcp.GetStream().ReadAsync(message, p, size - p, ct);
        if (dp == 0) throw new ConnectionFailure();
        p += dp;
      }
      while (p < size);
      var result = new string(Encoding.ASCII.GetChars(message));
      Console.WriteLine(
        $"{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}: Recv from {Name}/{Id}: " +
        (result.Length < 500? result: $"{result.Length} bytes"));
      return result;
    }

    public TimeSpan CommunicationTimeout { get; set; } = TimeSpan.FromSeconds(0.5);

    private async Task Write(string message) {
      using (var cts = new CancellationTokenSource(CommunicationTimeout)) {
        Console.WriteLine(
          $"{(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds}: Send to {Name}/{Id}: " +
          (message.Length < 500? message: $"{message.Length} bytes"));
        var data = Encoding.ASCII.GetBytes($"{message.Length}:{message}");
        await Tcp.GetStream().WriteAsync(data, 0, data.Length, cts.Token);
      }
    }
    
    protected override async Task HandshakeImpl(Game.Game game) {
      using (var cts = new CancellationTokenSource(CommunicationTimeout)) {
        var request = JObject.Parse(await Read(cts.Token));
        Name = (string)request["me"];
      }
      var response = new JObject() {
        ["you"] = Name
      };
      await Write(response.ToString(Formatting.None));
    }

    protected override async Task SetupImpl(Game.Game game) {
      var data = new JObject() {
        ["punter"] = Id,
        ["punters"] = game.PunterCount,
        ["map"] = JObject.FromObject(game.World)
      };
      await Write(data.ToString(Formatting.None));
      using (var cts = new CancellationTokenSource(game.SetupTimeout)) {
        var response = JObject.Parse(await Read(cts.Token));
        if ((int)response["ready"] != Id)
          throw new Exception($"Wrong ready id {response["ready"]} for {Id}");
      }
    }

    protected override async Task<Game.AMove> NextMoveImpl(Game.Game game) {
      var data = new JObject() {
        ["move"] = new JObject() {
          ["moves"] = JArray.FromObject(game.State.LastMoves)
        }
      };
      await Write(data.ToString(Formatting.None));
      using (var cts = new CancellationTokenSource(game.MoveTimeout)) {
        return JsonConvert.DeserializeObject<Game.AMove>(await Read(cts.Token));
      }
    }

    protected override async Task ReportScoreImpl(Game.AMove[] lastMoves, int[] scores) {
      var scoresData = new JObject[scores.Length];
      for (var i = 0; i < scores.Length; ++i) {
        scoresData[i] = new JObject() {
          ["punter"] = i,
          ["score"] = scores[i]
        };
      }
      var data = new JObject() {
        ["stop"] = new JObject() {
          ["moves"] = JArray.FromObject(lastMoves),
          ["scores"] = JArray.FromObject(scoresData)
        }
      };
      await Write(data.ToString(Formatting.None));
    }

    public bool Connected =>
      Tcp != null
      && Tcp.Connected
      && Tcp.Client != null
      && (!Tcp.Client.Poll (0, SelectMode.SelectRead)
          || Tcp.Client.Available > 0);
    
    public override void Dispose() {
      Tcp.Dispose();
    }
  }
}