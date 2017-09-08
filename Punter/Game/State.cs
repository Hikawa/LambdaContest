using System;
using System.Collections.Generic;
using System.Linq;

namespace Punter.Game {
  public sealed class State {
    public Spec.IWorld World { get; }
    public Dictionary<Spec.River, int> RiverOwners { get; } = new Dictionary<Spec.River, int>();
    public AMove[] LastMoves { get; }
    public IEnumerable<Spec.River> FreeRivers => World.Rivers.Where(r => !RiverOwners.ContainsKey(r));

    public int RiverOwner(Spec.River r) {
      int owner;
      if (!RiverOwners.TryGetValue(r, out owner))
        owner = -1;
      return owner;
    }

    public State(Spec.IWorld world, int punterCount) {
      World = world;
      LastMoves = new AMove[punterCount];
      for (var i = 0; i < punterCount; ++i)
        LastMoves[i] = new Pass(i);
    }

    public void ApplyMove(AMove move) => move.ApplyTo(this);

    public void Log() {
      Console.WriteLine("State:");
      foreach (var o in RiverOwners) { Console.WriteLine($"{o.Key} was taken by {o.Value}"); }
      Console.WriteLine();
    }

    public int Score(int punter) {
      var result = 0;
      foreach (var mine in World.Mines) {
        var visited = new bool[World.SiteArray.Count];
        for (var i = 0; i < visited.Length; ++i)
          visited[i] = false;
        visited[mine.Id] = true;
        var opened = new HashSet<int> {mine.Id};
        while (opened.Count > 0) {
          var newOpened = new HashSet<int>();
          foreach (var s in opened) {
            foreach (var n in World.SiteArray[s].RiversTo) {
              if (RiverOwner(new Spec.River(s, n)) != punter || visited[n]) continue;
              visited[n] = true;
              newOpened.Add(n);
              result += mine.Distances[n] * mine.Distances[n];
            }
          }
          opened = newOpened;
        }
      }
      return result;
    }
  }
}