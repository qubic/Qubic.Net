namespace Qubic.Rpc.Models;

/// <summary>
/// An issued asset with metadata.
/// </summary>
public sealed class IssuedAssetInfo
{
    public IssuedAssetData Data { get; set; } = new();
    public AssetInfoMeta Info { get; set; } = new();
}
