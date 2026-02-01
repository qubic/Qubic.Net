namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic asset ownership record.
/// This tracks who owns shares of an asset.
/// Size: 48 bytes
/// </summary>
public sealed class AssetOwnershipRecord
{
    /// <summary>
    /// The owner's public key (32 bytes).
    /// </summary>
    public required byte[] OwnerPublicKey { get; init; }

    /// <summary>
    /// Record type (always Ownership = 2).
    /// </summary>
    public AssetRecordType Type => AssetRecordType.Ownership;

    /// <summary>
    /// Index of the contract managing this ownership (0 for none).
    /// </summary>
    public required ushort ManagingContractIndex { get; init; }

    /// <summary>
    /// Index of the corresponding issuance record.
    /// </summary>
    public required uint IssuanceIndex { get; init; }

    /// <summary>
    /// Number of shares owned.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// Gets the owner identity.
    /// </summary>
    public QubicIdentity Owner => QubicIdentity.FromPublicKey(OwnerPublicKey);
}
