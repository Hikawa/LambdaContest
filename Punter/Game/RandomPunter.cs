using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Punter.Game {
  public class RandomPunter: APunter {
    private static readonly Random r = new Random();

    public RandomPunter(int id)
      : base(id) 
    {}

    protected override Task HandshakeImpl(Game game) {
      return Task.FromResult<object>(null);
    }
    
    protected override Task SetupImpl(Game game) {
      return Task.FromResult<object>(null);
    }

    protected override Task<AMove> NextMoveImpl(Game game) {
      var free = new List<Spec.River>(game.State.FreeRivers);
      if (free.Count == 0)
        return Task.FromResult<AMove>(new Pass(Id));
      var ri = r.Next() % free.Count;
      return Task.FromResult<AMove>(new Claim(Id, free[ri]));
    }

    protected override Task ReportScoreImpl(AMove[] lastMoves, int[] scores) {
      return Task.FromResult<object>(null);
    }

    public override void Dispose() {}
  }
}