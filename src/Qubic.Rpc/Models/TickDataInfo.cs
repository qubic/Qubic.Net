namespace Qubic.Rpc.Models;

/// <summary>
/// Tick data from the archive.
/// </summary>
public sealed class TickDataInfo
{
    public uint TickNumber { get; set; }
    public uint Epoch { get; set; }
    public uint ComputorIndex { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public string VarStruct { get; set; } = string.Empty;
    public string TimeLock { get; set; } = string.Empty;
    public List<string> TransactionHashes { get; set; } = [];
    public List<string> ContractFees { get; set; } = [];
    public string Signature { get; set; } = string.Empty;
}
