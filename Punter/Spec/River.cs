using System;
using Newtonsoft.Json;

namespace Punter.Spec {
  public sealed class River: IEquatable<River> {
    [JsonProperty(PropertyName = "source")]
    public int Source { get; }

    [JsonProperty(PropertyName = "target")]
    public int Target { get; }

    [JsonIgnore]
    public Tuple<int, int> Canonical => (Source < Target)? Tuple.Create(Source, Target): Tuple.Create(Target, Source);

    public River(int source, int target) {
      Source = source;
      Target = target;
    }

    public override int GetHashCode() => Canonical.GetHashCode();

    public bool Equals(River other) => Canonical.Equals(other?.Canonical);
    public override bool Equals(object obj) => Canonical.Equals((obj as River)?.Canonical);

    public static bool operator==(River x, River y) => (x == null)? (y == null): x.Equals(y);
    public static bool operator!=(River x, River y) => (x == null)? (y != null): !x.Equals(y);

    public override string ToString() => $"[{Source} - {Target}]";
  }
}