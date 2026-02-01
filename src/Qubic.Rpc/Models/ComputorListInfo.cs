namespace Qubic.Rpc.Models;

/// <summary>
/// A computor list for a specific epoch.
/// </summary>
public sealed class ComputorListInfo
{
    public uint Epoch { get; set; }
    public uint TickNumber { get; set; }
    public List<string> Identities { get; set; } = [];
    public string Signature { get; set; } = string.Empty;
}
