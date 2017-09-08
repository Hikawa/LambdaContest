using System;

namespace Punter.Game {
  public sealed class InvalidMoveException: Exception {
    public InvalidMoveException(string m): base(m) {}
  }
}