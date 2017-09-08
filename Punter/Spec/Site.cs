using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Punter.Spec {
  public sealed class Site: ISite {
    [JsonProperty(PropertyName = "id")]
    public int Id { get; }

    [JsonProperty(PropertyName = "x")]
    public double X { get; }

    [JsonProperty(PropertyName = "y")]
    public double Y { get; }

    [JsonIgnore]
    public ImmutableHashSet<int> RiversTo { get; set; }

    IImmutableSet<int> ISite.RiversTo => RiversTo;

    public Site(int id, double x = 0.0, double y = 0.0) {
      Id = id;
      X = x;
      Y = y;
    }
  }
}