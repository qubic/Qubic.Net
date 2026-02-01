namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic asset issuance record.
/// This defines an asset in the universe.
/// Size: 48 bytes
/// </summary>
public sealed class AssetIssuanceRecord
{
    /// <summary>
    /// The issuer's public key (32 bytes).
    /// </summary>
    public required byte[] IssuerPublicKey { get; init; }

    /// <summary>
    /// Record type (always Issuance = 1).
    /// </summary>
    public AssetRecordType Type => AssetRecordType.Issuance;

    /// <summary>
    /// The asset name (up to 7 characters).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Number of decimal places for display.
    /// </summary>
    public required sbyte NumberOfDecimalPlaces { get; init; }

    /// <summary>
    /// Unit of measurement (up to 7 characters, SI units).
    /// </summary>
    public required string UnitOfMeasurement { get; init; }

    /// <summary>
    /// Gets the issuer identity.
    /// </summary>
    public QubicIdentity Issuer => QubicIdentity.FromPublicKey(IssuerPublicKey);
}
