namespace Qubic.Core.Entities;

/// <summary>
/// Represents an asset issuance event.
/// </summary>
public sealed class AssetIssuanceEvent
{
    /// <summary>
    /// The issuer's public key (32 bytes).
    /// </summary>
    public required byte[] IssuerPublicKey { get; init; }

    /// <summary>
    /// Number of shares issued.
    /// </summary>
    public required long NumberOfShares { get; init; }

    /// <summary>
    /// Index of the contract managing this asset (0 for none).
    /// </summary>
    public required long ManagingContractIndex { get; init; }

    /// <summary>
    /// The asset name (up to 7 characters).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Number of decimal places.
    /// </summary>
    public required sbyte NumberOfDecimalPlaces { get; init; }

    /// <summary>
    /// Unit of measurement (up to 7 characters).
    /// </summary>
    public required string UnitOfMeasurement { get; init; }

    /// <summary>
    /// Gets the issuer identity.
    /// </summary>
    public QubicIdentity Issuer => QubicIdentity.FromPublicKey(IssuerPublicKey);
}
