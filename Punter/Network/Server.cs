using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading.Tasks;
using Punter.Game;

namespace Punter.Network {
  public class Server {
    private object lock_ = new object();
    private TcpListener tcpListener_;
    public Game.Game Game { get; private set; }

    public async Task RunGame(int listenPort, Spec.IWorld world, int punterCount) {
      tcpListener_ = TcpListener.Create(listenPort);
      tcpListener_.ExclusiveAddressUse = true;
      tcpListener_.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
      tcpListener_.Start();
      using (Game = new Game.Game(world, punterCount)) {
        var handlers = new List<Task>();
        var clients = new List<Tuple<Client, Task>>();
        do {
          foreach (var id in Game.FreeSlots) {
            var client = new Client(id);
            Game.SetPunter(client);
            var h = SetupClient(client);
            handlers.Add(h);
            clients.Add(Tuple.Create(client, h));
          }
          await Task.WhenAny(handlers);
          handlers.RemoveAll(h => h.IsCompleted);
          clients.RemoveAll(
            c => {
              if (!c.Item2.IsCompleted || c.Item1.Connected) return false;
              Game.ResetPunter(c.Item1.Id);
              c.Item1.Dispose();
              return true;
            });
        }
        while (!Game.IsReady);
        tcpListener_.Stop();
        try { await Game.Run(); }
        finally {
          await Game.ReportScores();
        }
      }
    }

    private async Task SetupClient(Client client) {
      try {
        client.Tcp = await tcpListener_.AcceptTcpClientAsync();
        await client.Setup(Game);
      } catch (Exception e) {
        Console.WriteLine($"Client init error: {e}");
      }
    }
  }
}
