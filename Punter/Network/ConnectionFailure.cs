using System;

namespace Punter.Network {
  public class ConnectionFailure: Exception {
    public ConnectionFailure(): base("ConnectionFailure") {}
  }
}