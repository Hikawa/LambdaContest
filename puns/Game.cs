using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Puns.Game {

public sealed class InvalidMoveException: Exception {
  public InvalidMoveException(string m) : base(m) {
  }
}

[JsonConverter(typeof(MoveJsonConverter))]
public abstract class AMove {
  public int Punter { get; }

  public abstract string MoveType { get; }

  public abstract bool IsValid(State state);
  public virtual void ApplyTo(State state) {
    state.LastMoves [Punter] = this;
  }
  //public abstract dynamic ToJson();
  public void WriteJson(JsonWriter writer, JsonSerializer serializer) {
    writer.WriteStartObject();
    writer.WritePropertyName(MoveType);
    writer.WriteStartObject();
    writer.WritePropertyName("punter");
    serializer.Serialize(writer, Punter);
    WriteJsonImpl(writer, serializer);
    writer.WriteEndObject();
    writer.WriteEndObject();
  }

  protected abstract void WriteJsonImpl(JsonWriter writer, JsonSerializer serializer);

  protected AMove(int punter) {
    Punter = punter;
  }

  //public AMove Fix(State state, out bool wasValid) => (wasValid = IsValid(state))? this: new Pass(Punter);
}

public sealed class Pass: AMove {

  public override string MoveType => "pass";

  public override bool IsValid(State state) => true;

  public Pass(int punter): base(punter) {
  }

  public override void ApplyTo(State state) {
    base.ApplyTo(state);
    Console.WriteLine($"Punter {Punter} passed");
  }

  protected override void WriteJsonImpl(JsonWriter writer, JsonSerializer serializer) {
  }
}

public sealed class Claim: AMove {
  public Spec.River River { get; }
  public override string MoveType => "claim";

  public override bool IsValid(State state) {
    return true;
  }

  public int Source => River.Source;
  public int Target => River.Target;

  public override void ApplyTo(State state) {
    if (!state.World.RiverSet.Contains(River))
      throw new InvalidMoveException($"Invalid move {River}: no such river");

    if (state.RiverOwners.ContainsKey(River))
      throw new InvalidMoveException($"Invalid move {River} for {Punter}: river has owner {state.RiverOwners[River]}");

    base.ApplyTo(state);
    state.RiverOwners [River] = Punter;
    Console.WriteLine($"Punter {Punter} claims {River}");
  }

  public Claim(int punter, Spec.River river): base(punter) {
    River = river;
  }

  protected override void WriteJsonImpl(JsonWriter writer, JsonSerializer serializer) {
    writer.WritePropertyName("source");
    serializer.Serialize(writer, Source);
    writer.WritePropertyName("target");
    serializer.Serialize(writer, Target);
  }
}

public sealed class MoveJsonConverter: JsonConverter {
  public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
    (value as AMove).WriteJson(writer, serializer);
  }

  public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
    JObject jsonObject = JObject.Load(reader);
    var prop = jsonObject.Properties().First();
    var punter = (int)prop.Value["punter"];
    switch (prop.Name) {
    case "pass":
      return new Pass(punter);
    case "claim":
      return new Claim(punter, new Puns.Spec.River((int)prop.Value["source"], (int)prop.Value["target"]));
    default:
      throw new Exception($"Unknown move type {prop.Name}");
    }
  }

  public override bool CanConvert(Type objectType) {
    return typeof(AMove).IsAssignableFrom(objectType);
  }
}

public sealed class State {
  public Spec.IWorld World { get; }
  public Dictionary<Spec.River, int> RiverOwners { get; } = new Dictionary<Spec.River, int>();
  public AMove[] LastMoves { get; }
  public IEnumerable<Spec.River> FreeRivers {
    get {
      foreach (var r in World.Rivers)
        if (!RiverOwners.ContainsKey(r))
          yield return r;
    }
  }
  public int RiverOwner(Spec.River r) {
    int owner;
    if (!RiverOwners.TryGetValue(r, out owner))
      owner = -1;
    return owner;
  }

  public State(Spec.IWorld world, int punterCount) {
    World = world;
    LastMoves = new AMove[punterCount];
    for (int i = 0; i < punterCount; ++i)
      LastMoves [i] = new Pass(i);
  }

  public void ApplyMove(AMove move) => move.ApplyTo(this);

  public void Log() {
    Console.WriteLine("State:");
    foreach (var o in RiverOwners) {
      Console.WriteLine($"{o.Key} was taken by {o.Value}");
    }
    Console.WriteLine();
  }

  public int Score(int punter) {
    int result = 0;
    foreach (var mine in World.Mines) {
      bool[] visited = new bool[World.SiteArray.Count];
      for (int i = 0; i < visited.Length; ++i)
        visited [i] = false;
      visited [mine.Id] = true;
      HashSet<int> opened = new HashSet<int>();
      opened.Add(mine.Id);
      while (opened.Count > 0) {
        HashSet<int> newOpened = new HashSet<int>();
        foreach (var s in opened) {
          foreach (var n in World.SiteArray[s].RiversTo) {
            if (RiverOwner(new Spec.River(s, n)) == punter && !visited [n]) {
              visited [n] = true;
              newOpened.Add(n);
              result += mine.Distances [n] * mine.Distances [n];
            }
          }
        }
        opened = newOpened;
      }
    }
    return result;
  }
}

public abstract class APunter {
  public int Id { get; set; }
  public string Name { get; set; }

  public abstract Task Setup(Game game);
  public abstract Task<AMove> NextMove(State state);
  public abstract Task ReportScore(State state, int[] scores);
}

public class RandomPunter: APunter {
  static private Random r = new Random();

  public override Task Setup(Game game) {
    return Task.FromResult(new Object());
  }

  public override Task<AMove> NextMove(State state) {
    var free = new List<Spec.River>(state.FreeRivers);
    if (free.Count == 0)
      return Task.FromResult<AMove>(new Pass(Id));
    var ri = r.Next() % free.Count;
    return Task.FromResult<AMove>(new Claim(Id, free [ri]));
  }

  public override Task ReportScore(State state, int[] scores) {
    return Task.FromResult(new Object());
  }
}

public sealed class Game {
  public State State { get; }
  public Spec.IWorld World => State.World;
  public APunter[] Punters { get; }
  public int PunterCount => Punters.Length;

  public Game(Spec.IWorld world, int punterCount) {
    State = new State(world, punterCount);
    Punters = new APunter[punterCount];
  }

  public bool AddPunter(APunter punter) {
    for (int i = 0; i < Punters.Length; ++i)
      if (Punters [i] == null) {
        Punters [i] = punter;
        punter.Id = i;
        return true;
      }
    return false;
  }

  public bool IsFull {
    get {
      foreach (var p in Punters)
        if (p == null)
          return false;
      return true;
    }
  }

  public async Task Run() {
    foreach (var p in Punters) {
      await p.Setup(this);
    }
    for (int step = 0; step < World.Rivers.Count / PunterCount; ++step) {
      foreach (var p in Punters) {
        State.ApplyMove(await p.NextMove(State));
      }
      //State.Log();
    }
    int[] scores = new int[PunterCount];
    for (int i = 0; i < PunterCount; ++i)
      scores [i] = State.Score(i);
    foreach (var p in Punters) {
      await p.ReportScore(State, scores);
      State.LastMoves[p.Id] = new Pass(p.Id);
    }
  }
};

}
