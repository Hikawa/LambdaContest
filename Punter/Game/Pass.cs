using System;
using Newtonsoft.Json;

namespace Punter.Game {
  public sealed class Pass: AMove {
    public override string MoveType => "pass";

    public override bool IsValid(State state) => true;

    public Pass(int punter): base(punter) {}

    public override void ApplyTo(State state) {
      base.ApplyTo(state);
      Console.WriteLine($"Punter {Punter} passed");
    }

    protected override void WriteJsonImpl(JsonWriter writer, JsonSerializer serializer) {}
  }
}