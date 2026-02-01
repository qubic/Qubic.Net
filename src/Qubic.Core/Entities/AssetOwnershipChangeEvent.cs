namespace Qubic.Core.Entities;

/// <summary>
/// Represents an asset ownership change event.
/// </summary>
public sealed class AssetOwnershipChangeEvent
{
    /// <summary>
    /// The previous owner's public key (32 bytes).
    /// </summary>
    public required byte[] SourcePublicKey { get; init; }

    /// <summary>
    /// The new owner's public key (32 bytes).
    /// </summary>
    public required byte[] DestinationPublicKey { get; init; }

    /// <summary>
    /// The asset issuer's public key (32 bytes).
    /// </summary>
    public required byte[] IssuerPublicKey { get; init; }

    /// <summary>
    /// Number of shares transferred.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// Index of the contract managing this asset.
    /// </summary>
    public required long ManagingContractIndex { get; init; }

    /// <summary>
    /// The asset name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Number of decimal places.
    /// </summary>
    public required sbyte NumberOfDecimalPlaces { get; init; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public required string UnitOfMeasurement { get; init; }

    /// <summary>
    /// Gets the source (previous owner) identity.
    /// </summary>
    public QubicIdentity Source => QubicIdentity.FromPublicKey(SourcePublicKey);

    /// <summary>
    /// Gets the destination (new owner) identity.
    /// </summary>
    public QubicIdentity Destination => QubicIdentity.FromPublicKey(DestinationPublicKey);

    /// <summary>
    /// Gets the issuer identity.
    /// </summary>
    public QubicIdentity Issuer => QubicIdentity.FromPublicKey(IssuerPublicKey);
}
