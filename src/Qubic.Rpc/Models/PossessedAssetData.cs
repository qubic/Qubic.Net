namespace Qubic.Rpc.Models;

public sealed class PossessedAssetData
{
    public string PossessorIdentity { get; set; } = string.Empty;
    public uint Type { get; set; }
    public uint ManagingContractIndex { get; set; }
    public uint OwnershipIndex { get; set; }
    public string NumberOfUnits { get; set; } = "0";
    public OwnedAssetData? OwnedAsset { get; set; }
}
