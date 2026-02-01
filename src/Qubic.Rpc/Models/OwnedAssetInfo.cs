namespace Qubic.Rpc.Models;

/// <summary>
/// An owned asset with ownership data.
/// </summary>
public sealed class OwnedAssetInfo
{
    public OwnedAssetData Data { get; set; } = new();
    public AssetInfoMeta Info { get; set; } = new();
}
