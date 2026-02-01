namespace Qubic.Core.Entities;

/// <summary>
/// Represents an asset ownership managing contract change event.
/// </summary>
public sealed class AssetOwnershipManagingContractChangeEvent
{
    /// <summary>
    /// The owner's public key (32 bytes).
    /// </summary>
    public required byte[] OwnershipPublicKey { get; init; }

    /// <summary>
    /// The asset issuer's public key (32 bytes).
    /// </summary>
    public required byte[] IssuerPublicKey { get; init; }

    /// <summary>
    /// The previous managing contract index.
    /// </summary>
    public required uint SourceContractIndex { get; init; }

    /// <summary>
    /// The new managing contract index.
    /// </summary>
    public required uint DestinationContractIndex { get; init; }

    /// <summary>
    /// Number of shares affected.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// The asset name.
    /// </summary>
    public required string AssetName { get; init; }
}
