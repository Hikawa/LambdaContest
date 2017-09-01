using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace Puns.Spec {

public interface ISite {
  int Id { get; }
  double X { get; }
  double Y { get; }

  IImmutableSet<int> RiversTo { get; }

}

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

public sealed class River: IEquatable<River> {
  [JsonProperty(PropertyName = "source")]
  public int Source { get; }

  [JsonProperty(PropertyName = "target")]
  public int Target { get; }

  [JsonIgnore]
  public Tuple<int, int> Canonical => (Source < Target)? Tuple.Create(Source, Target) : Tuple.Create(Target, Source);

  public River(int source, int target) {
    Source = source;
    Target = target;
  }

  public override int GetHashCode() => Canonical.GetHashCode();

  public bool Equals(River other) => Canonical.Equals(other?.Canonical);
  public override bool Equals(Object obj) => Canonical.Equals((obj as River)?.Canonical);

  public static bool operator == (River x, River y) => (x == null)? (y == null): x.Equals(y);
  public static bool operator != (River x, River y) => (x == null)? (y != null): !x.Equals(y);

  public override string ToString() => $"[{Source} - {Target}]";
}

public class Mine {
  public ISite Site { get; }
  public int Id => Site.Id;
  public int[] Distances { get; }

  public Mine(IWorld world, int id) {
    Site = world.SiteArray[id];
    Distances = new int[world.SiteArray.Count];
    for (int i = 0; i < Distances.Length; ++i)
      Distances [i] = -1;
    Distances[Id] = 0;
    HashSet<int> opened = new HashSet<int>() { Id };
    int dist = 0;
    while (opened.Count > 0) {
      HashSet<int> newOpened = new HashSet<int>();
      dist++;
      foreach (var s in opened) {
        foreach (var n in world.SiteArray[s].RiversTo) {
          if (Distances [n] == -1) {
            Distances [n] = dist;
            newOpened.Add(n);
          }
        }
      }
      opened = newOpened;
    }
  }
}


public interface IWorld {
  IReadOnlyList<ISite> Sites { get; }
  IReadOnlyList<ISite> SiteArray { get; }

  IReadOnlyList<River> Rivers { get; }
  IReadOnlyCollection<River> RiverSet { get; }

  IReadOnlyList<Mine> Mines { get; }
  IReadOnlyCollection<Mine> MineSet { get; }
}

public sealed class World: IWorld {
  [JsonProperty(PropertyName = "sites")]
  public ImmutableList<Site> Sites { get; }
  IReadOnlyList<ISite> IWorld.Sites => Sites;

  [JsonIgnore]
  public ImmutableArray<Site> SiteArray { get; }
  IReadOnlyList<ISite> IWorld.SiteArray => SiteArray;

  [JsonProperty(PropertyName = "rivers")]
  public ImmutableList<River> Rivers { get; }
  IReadOnlyList<River> IWorld.Rivers => Rivers;

  [JsonIgnore]
  public ImmutableHashSet<River> RiverSet { get; }
  IReadOnlyCollection<River> IWorld.RiverSet => RiverSet;

  [JsonIgnore]
  public ImmutableList<Mine> Mines { get; }
  IReadOnlyList<Mine> IWorld.Mines => Mines;

  [JsonProperty(PropertyName = "mines")]
  public ImmutableList<int> MineIds => Mines.ConvertAll(m => m.Id);

  [JsonIgnore]
  public ImmutableHashSet<Mine> MineSet { get; }
  IReadOnlyCollection<Mine> IWorld.MineSet => MineSet;

  [JsonConstructor]
  public World(ImmutableList<Site> sites, ImmutableList<River> rivers, ImmutableList<int> mines) {
    Sites = sites;
    Rivers = rivers;

    var siteArray = new List<Site>();
    foreach (var r in sites) {
      while (r.Id >= siteArray.Count)
        siteArray.Add(null);
      siteArray[r.Id] = r;
    }
    SiteArray = siteArray.ToImmutableArray();

    RiverSet = Rivers.ToImmutableHashSet();

    var riversTo = new HashSet<int>[SiteArray.Length];
    for (int i = 0; i < SiteArray.Length; ++i)
      riversTo [i] = new HashSet<int>();
    foreach (var r in rivers) {
      riversTo [r.Source].Add(r.Target);
      riversTo [r.Target].Add(r.Source);
    }
    foreach (var s in sites) {
      s.RiversTo = riversTo[s.Id].ToImmutableHashSet();
    }

    Mines = mines.ConvertAll(id => new Mine(this, id));
    MineSet = Mines.ToImmutableHashSet();
  }
}

}