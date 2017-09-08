using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json;

namespace Punter.Spec {
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
      for (var i = 0; i < SiteArray.Length; ++i)
        riversTo[i] = new HashSet<int>();
      foreach (var r in rivers) {
        riversTo[r.Source].Add(r.Target);
        riversTo[r.Target].Add(r.Source);
      }
      foreach (var s in sites) { s.RiversTo = riversTo[s.Id].ToImmutableHashSet(); }

      Mines = mines.ConvertAll(id => new Mine(this, id));
      MineSet = Mines.ToImmutableHashSet();
    }
  }
}
