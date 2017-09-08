using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Punter.Game {
  public sealed class Game: IDisposable {
    public State State { get; }
    public Spec.IWorld World => State.World;
    public APunter[] Punters { get; }
    public int PunterCount => Punters.Length;
    public int[] CurrentScores { get; }
    public AMove[] LastMovesSnapshot { get; private set; }

    public TimeSpan SetupTimeout { get; set; } = TimeSpan.FromSeconds(10.0);
    public TimeSpan MoveTimeout { get; set; } = TimeSpan.FromSeconds(1.0);

    public Game(Spec.IWorld world, int punterCount) {
      State = new State(world, punterCount);
      Punters = new APunter[punterCount];
      CurrentScores = new int[punterCount];
      LastMovesSnapshot = new AMove[punterCount];
      for (var i = 0; i < punterCount; ++i)
        LastMovesSnapshot[i] = new Pass(i);
    }

    public void SetPunter(APunter punter) {
      Punters[punter.Id] = punter;
    }

    public void ResetPunter(int id) {
      Punters[id]?.Dispose();
      Punters[id] = null;
    }
    
    public bool IsReady => Punters.All(p => p != null && p.CurrentStatus == APunter.Status.Initialized);

    public IEnumerable<int> FreeSlots {
      get {
        var result = new List<int>();
        for (var i = 0; i < Punters.Length; ++i)
          if (Punters[i] == null) result.Add(i);
        return result;
      }
    }

    public async Task Run() {
      for (var step = 0; step < World.Rivers.Count / PunterCount; ++step) {
        foreach (var p in Punters) { State.ApplyMove(await p.NextMove(this)); }
        for (var i = 0; i < PunterCount; ++i)
          CurrentScores[i] = State.Score(i);
        LastMovesSnapshot = (AMove[])State.LastMoves.Clone();
      }
    }
    
    public async Task ReportScores() {
      await Task.WhenAll(Punters.Select(async p => {
        await p.ReportScore(this, LastMovesSnapshot, CurrentScores);
        LastMovesSnapshot[p.Id] = new Pass(p.Id);
      }));
    }

    public void Dispose() {
      foreach (var aPunter in Punters) { aPunter.Dispose(); }
    }
  };
}
