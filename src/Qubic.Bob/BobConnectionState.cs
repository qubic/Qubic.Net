namespace Qubic.Bob;

/// <summary>
/// Connection state of the <see cref="BobWebSocketClient"/>.
/// </summary>
public enum BobConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Reconnecting
}
