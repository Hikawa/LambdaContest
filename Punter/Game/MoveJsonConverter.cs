using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Punter.Game {
  public sealed class MoveJsonConverter: JsonConverter {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
      (value as AMove)?.WriteJson(writer, serializer);
    }

    public override object ReadJson(
      JsonReader reader,
      Type objectType,
      object existingValue,
      JsonSerializer serializer)
    {
      var jsonObject = JObject.Load(reader);
      var prop = jsonObject.Properties().First();
      var punter = (int)prop.Value["punter"];
      switch (prop.Name) {
      case "pass": return new Pass(punter);
      case "claim": return new Claim(punter, new Spec.River((int)prop.Value["source"], (int)prop.Value["target"]));
      default: throw new Exception($"Unknown move type {prop.Name}");
      }
    }

    public override bool CanConvert(System.Type objectType) {
      return typeof(AMove).IsAssignableFrom(objectType);
    }
  }
}