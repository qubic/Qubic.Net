namespace Qubic.Core.Entities;

/// <summary>
/// Full tick data structure as defined in the Qubic protocol.
/// Size: 344 bytes
/// </summary>
public sealed class Tick
{
    /// <summary>
    /// Index of the computor that created this tick (0-675).
    /// </summary>
    public required ushort ComputorIndex { get; init; }

    /// <summary>
    /// The epoch this tick belongs to.
    /// </summary>
    public required ushort Epoch { get; init; }

    /// <summary>
    /// The tick number.
    /// </summary>
    public required uint TickNumber { get; init; }

    /// <summary>
    /// Millisecond component of the timestamp.
    /// </summary>
    public required ushort Millisecond { get; init; }

    /// <summary>
    /// Second component of the timestamp (0-59).
    /// </summary>
    public required byte Second { get; init; }

    /// <summary>
    /// Minute component of the timestamp (0-59).
    /// </summary>
    public required byte Minute { get; init; }

    /// <summary>
    /// Hour component of the timestamp (0-23).
    /// </summary>
    public required byte Hour { get; init; }

    /// <summary>
    /// Day component of the timestamp (1-31).
    /// </summary>
    public required byte Day { get; init; }

    /// <summary>
    /// Month component of the timestamp (1-12).
    /// </summary>
    public required byte Month { get; init; }

    /// <summary>
    /// Year component of the timestamp (offset from 2000).
    /// </summary>
    public required byte Year { get; init; }

    /// <summary>
    /// Previous resource testing digest.
    /// </summary>
    public required uint PrevResourceTestingDigest { get; init; }

    /// <summary>
    /// Salted resource testing digest.
    /// </summary>
    public required uint SaltedResourceTestingDigest { get; init; }

    /// <summary>
    /// Previous transaction body digest.
    /// </summary>
    public required uint PrevTransactionBodyDigest { get; init; }

    /// <summary>
    /// Salted transaction body digest.
    /// </summary>
    public required uint SaltedTransactionBodyDigest { get; init; }

    /// <summary>
    /// Previous spectrum (accounts) digest (32 bytes).
    /// </summary>
    public required byte[] PrevSpectrumDigest { get; init; }

    /// <summary>
    /// Previous universe (assets) digest (32 bytes).
    /// </summary>
    public required byte[] PrevUniverseDigest { get; init; }

    /// <summary>
    /// Previous computer (contracts) digest (32 bytes).
    /// </summary>
    public required byte[] PrevComputerDigest { get; init; }

    /// <summary>
    /// Salted spectrum digest (32 bytes).
    /// </summary>
    public required byte[] SaltedSpectrumDigest { get; init; }

    /// <summary>
    /// Salted universe digest (32 bytes).
    /// </summary>
    public required byte[] SaltedUniverseDigest { get; init; }

    /// <summary>
    /// Salted computer digest (32 bytes).
    /// </summary>
    public required byte[] SaltedComputerDigest { get; init; }

    /// <summary>
    /// Transaction digest for this tick (32 bytes).
    /// </summary>
    public required byte[] TransactionDigest { get; init; }

    /// <summary>
    /// Expected transaction digest for the next tick (32 bytes).
    /// </summary>
    public required byte[] ExpectedNextTickTransactionDigest { get; init; }

    /// <summary>
    /// Signature of the tick (64 bytes).
    /// </summary>
    public required byte[] Signature { get; init; }

    /// <summary>
    /// Gets the timestamp as a DateTime (UTC).
    /// </summary>
    public DateTime Timestamp => new DateTime(2000 + Year, Month, Day, Hour, Minute, Second, Millisecond, DateTimeKind.Utc);
}
