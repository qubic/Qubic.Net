using System.Net;

namespace Qubic.Network;

/// <summary>
/// Represents a Qubic network peer.
/// </summary>
public sealed class QubicPeer
{
    /// <summary>
    /// The peer's IP address.
    /// </summary>
    public required IPAddress Address { get; init; }

    /// <summary>
    /// The peer's port (default: 21841).
    /// </summary>
    public int Port { get; init; } = 21841;

    /// <summary>
    /// Whether this peer is currently reachable.
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    /// The last time this peer was successfully contacted.
    /// </summary>
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// The peer's reported current tick.
    /// </summary>
    public uint? CurrentTick { get; set; }

    public override string ToString() => $"{Address}:{Port}";
}
