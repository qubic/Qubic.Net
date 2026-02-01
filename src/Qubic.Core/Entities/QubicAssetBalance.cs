namespace Qubic.Core.Entities;

/// <summary>
/// Represents an asset balance for a specific identity (combines ownership info).
/// </summary>
public sealed class QubicAssetBalance
{
    /// <summary>
    /// The asset.
    /// </summary>
    public required QubicAsset Asset { get; init; }

    /// <summary>
    /// The identity holding this asset.
    /// </summary>
    public required QubicIdentity Owner { get; init; }

    /// <summary>
    /// The number of shares owned.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// Index of the contract managing this ownership.
    /// </summary>
    public ushort ManagingContractIndex { get; init; }
}
