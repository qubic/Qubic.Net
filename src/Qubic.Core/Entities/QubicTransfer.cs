namespace Qubic.Core.Entities;

/// <summary>
/// Represents a transfer record (used by Bob and RPC for history queries).
/// </summary>
public sealed class QubicTransfer
{
    /// <summary>
    /// The transaction hash.
    /// </summary>
    public required string TransactionHash { get; init; }

    /// <summary>
    /// The source identity.
    /// </summary>
    public required QubicIdentity Source { get; init; }

    /// <summary>
    /// The destination identity.
    /// </summary>
    public required QubicIdentity Destination { get; init; }

    /// <summary>
    /// The amount transferred in QU.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// The tick number when this transfer occurred.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// The timestamp of the transfer (Unix milliseconds).
    /// </summary>
    public long? Timestamp { get; init; }

    /// <summary>
    /// Whether the transfer was successful.
    /// </summary>
    public bool? Success { get; init; }
}
