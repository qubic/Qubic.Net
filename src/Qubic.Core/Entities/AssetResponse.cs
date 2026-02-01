namespace Qubic.Core.Entities;

/// <summary>
/// Asset response from the network including Merkle proof.
/// </summary>
public sealed class AssetResponse
{
    /// <summary>
    /// The asset record (issuance, ownership, or possession).
    /// </summary>
    public required object Record { get; init; }

    /// <summary>
    /// The record type.
    /// </summary>
    public required AssetRecordType RecordType { get; init; }

    /// <summary>
    /// The tick at which this data was retrieved.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// The universe index of this record.
    /// </summary>
    public required uint UniverseIndex { get; init; }

    /// <summary>
    /// Merkle tree siblings for verification (24 × 32 bytes).
    /// </summary>
    public byte[][]? Siblings { get; init; }
}
