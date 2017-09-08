using System;
using System.Linq;
using Newtonsoft.Json;

namespace Punter.Game {
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
        throw new InvalidMoveException(
          $"Invalid move {River} for {Punter}: river has owner {state.RiverOwners[River]}");

      base.ApplyTo(state);
      state.RiverOwners[River] = Punter;
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
}