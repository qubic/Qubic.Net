namespace Qubic.Core.Entities;

/// <summary>
/// Response from requesting entity information including Merkle proof.
/// </summary>
public sealed class EntityResponse
{
    /// <summary>
    /// The entity record.
    /// </summary>
    public required EntityRecord Entity { get; init; }

    /// <summary>
    /// The tick at which this data was retrieved.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// The spectrum index of this entity.
    /// </summary>
    public required int SpectrumIndex { get; init; }

    /// <summary>
    /// Merkle tree siblings for verification (24 × 32 bytes).
    /// </summary>
    public byte[][]? Siblings { get; init; }
}
