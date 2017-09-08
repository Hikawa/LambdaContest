using Newtonsoft.Json;

namespace Punter.Game {
  [JsonConverter(typeof(MoveJsonConverter))]
  public abstract class AMove {
    public int Punter { get; }

    public abstract string MoveType { get; }

    public abstract bool IsValid(State state);

    public virtual void ApplyTo(State state) {
      state.LastMoves[Punter] = this;
    }

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
}