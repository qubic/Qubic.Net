namespace Qubic.Rpc.Models;

public sealed class OwnedAssetData
{
    public string OwnerIdentity { get; set; } = string.Empty;
    public uint Type { get; set; }
    public uint ManagingContractIndex { get; set; }
    public uint IssuanceIndex { get; set; }
    public string NumberOfUnits { get; set; } = "0";
    public IssuedAssetData? IssuedAsset { get; set; }
}
