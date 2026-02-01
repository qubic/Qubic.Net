namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic asset (token) - simplified view combining issuance info.
/// </summary>
public sealed class QubicAsset
{
    /// <summary>
    /// The issuer identity of the asset.
    /// </summary>
    public required QubicIdentity Issuer { get; init; }

    /// <summary>
    /// The asset name (up to 7 characters).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The number of decimal places for display purposes.
    /// </summary>
    public sbyte NumberOfDecimalPlaces { get; init; }

    /// <summary>
    /// The unit of measurement suffix (up to 7 characters).
    /// </summary>
    public string? UnitOfMeasurement { get; init; }

    /// <summary>
    /// Returns the unique asset identifier string.
    /// </summary>
    public string AssetId => $"{Issuer}/{Name}";

    public override string ToString() => AssetId;
}
