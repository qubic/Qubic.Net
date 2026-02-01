namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic asset possession record.
/// This tracks who holds (possesses) shares of an asset.
/// Ownership and possession can differ (e.g., shares lent out).
/// Size: 48 bytes
/// </summary>
public sealed class AssetPossessionRecord
{
    /// <summary>
    /// The possessor's public key (32 bytes).
    /// </summary>
    public required byte[] PossessorPublicKey { get; init; }

    /// <summary>
    /// Record type (always Possession = 3).
    /// </summary>
    public AssetRecordType Type => AssetRecordType.Possession;

    /// <summary>
    /// Index of the contract managing this possession (0 for none).
    /// </summary>
    public required ushort ManagingContractIndex { get; init; }

    /// <summary>
    /// Index of the corresponding ownership record.
    /// </summary>
    public required uint OwnershipIndex { get; init; }

    /// <summary>
    /// Number of shares possessed.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// Gets the possessor identity.
    /// </summary>
    public QubicIdentity Possessor => QubicIdentity.FromPublicKey(PossessorPublicKey);
}
