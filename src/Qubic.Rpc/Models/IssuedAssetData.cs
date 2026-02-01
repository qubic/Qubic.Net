namespace Qubic.Rpc.Models;

public sealed class IssuedAssetData
{
    public string IssuerIdentity { get; set; } = string.Empty;
    public uint Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public int NumberOfDecimalPlaces { get; set; }
    public int[]? UnitOfMeasurement { get; set; }
}
