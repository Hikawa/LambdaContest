using System.Collections.Generic;

namespace Punter.Spec {
  public class Mine {
    public ISite Site { get; }
    public int Id => Site.Id;
    public int[] Distances { get; }

    public Mine(IWorld world, int id) {
      Site = world.SiteArray[id];
      Distances = new int[world.SiteArray.Count];
      for (var i = 0; i < Distances.Length; ++i)
        Distances[i] = -1;
      Distances[Id] = 0;
      var opened = new HashSet<int>() {Id};
      var dist = 0;
      while (opened.Count > 0) {
        var newOpened = new HashSet<int>();
        dist++;
        foreach (var s in opened) {
          foreach (var n in world.SiteArray[s].RiversTo) {
            if (Distances[n] != -1) continue;
            Distances[n] = dist;
            newOpened.Add(n);
          }
        }
        opened = newOpened;
      }
    }
  }
}