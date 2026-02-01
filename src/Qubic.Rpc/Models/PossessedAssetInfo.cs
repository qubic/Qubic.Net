namespace Qubic.Rpc.Models;

/// <summary>
/// A possessed asset with possession data.
/// </summary>
public sealed class PossessedAssetInfo
{
    public PossessedAssetData Data { get; set; } = new();
    public AssetInfoMeta Info { get; set; } = new();
}
