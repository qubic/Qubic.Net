namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic tick (block equivalent).
/// This is the simplified view; see TickData for the full protocol structure.
/// </summary>
public sealed class QubicTick
{
    /// <summary>
    /// The tick number.
    /// </summary>
    public required uint TickNumber { get; init; }

    /// <summary>
    /// The epoch this tick belongs to.
    /// </summary>
    public required ushort Epoch { get; init; }

    /// <summary>
    /// The timestamp of when this tick was created (Unix milliseconds).
    /// </summary>
    public required long Timestamp { get; init; }

    /// <summary>
    /// The tick leader's identity (computor that proposed this tick).
    /// </summary>
    public QubicIdentity? TickLeader { get; init; }

    /// <summary>
    /// The signature of the tick data.
    /// </summary>
    public byte[]? Signature { get; init; }

    /// <summary>
    /// Previous tick's digest.
    /// </summary>
    public byte[]? PreviousTickDigest { get; init; }

    /// <summary>
    /// Transaction digest for this tick.
    /// </summary>
    public byte[]? TransactionDigest { get; init; }

    /// <summary>
    /// The UTC DateTime of this tick.
    /// </summary>
    public DateTime TimestampUtc => DateTimeOffset.FromUnixTimeMilliseconds(Timestamp).UtcDateTime;
}
