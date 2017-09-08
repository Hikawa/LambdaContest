using System.Collections.Immutable;

namespace Punter.Spec {
  public interface ISite {
    int Id { get; }
    double X { get; }
    double Y { get; }

    IImmutableSet<int> RiversTo { get; }
  }
}