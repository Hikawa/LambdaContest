using System.Collections.Generic;

namespace Punter.Spec {
  public interface IWorld {
    IReadOnlyList<ISite> Sites { get; }
    IReadOnlyList<ISite> SiteArray { get; }

    IReadOnlyList<River> Rivers { get; }
    IReadOnlyCollection<River> RiverSet { get; }

    IReadOnlyList<Mine> Mines { get; }
    IReadOnlyCollection<Mine> MineSet { get; }
  }
}