namespace Qubic.Rpc.Models;

/// <summary>
/// An active IPO.
/// </summary>
public sealed class IpoInfo
{
    public uint ContractIndex { get; set; }
    public string AssetName { get; set; } = string.Empty;
}
