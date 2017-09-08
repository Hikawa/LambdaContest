using System;
using System.Threading;
using System.Threading.Tasks;

namespace Punter.Game {
  public abstract class APunter: IDisposable {
    public int Id { get; set; }
    public string Name { get; set; }
    public enum Status {
      Reserved,
      Connected,
      Handshaked,
      Initialized,
      Waiting,
      Moving,
      Finished
    }
    public Status CurrentStatus { get; private set; } = Status.Reserved;

    protected APunter(int id) {
      Id = id;
    }

    public async Task Setup(Game game) {
      CurrentStatus = Status.Connected;
      await HandshakeImpl(game);
      CurrentStatus = Status.Handshaked;
      await SetupImpl(game);
      CurrentStatus = Status.Initialized;
    }

    public async Task<AMove> NextMove(Game game) {
      CurrentStatus = Status.Moving;
      var result = await NextMoveImpl(game);
      CurrentStatus = Status.Waiting;
      return result;
    }

    public async Task ReportScore(Game game, AMove[] lastMoves, int[] scores) {
      CurrentStatus = Status.Finished;
      await ReportScoreImpl(lastMoves, scores);
    }

    protected abstract Task HandshakeImpl(Game game);
    protected abstract Task SetupImpl(Game game);
    protected abstract Task<AMove> NextMoveImpl(Game state);
    protected abstract Task ReportScoreImpl(AMove[] lastMoves, int[] scores);
    
    public abstract void Dispose();
  }
}
