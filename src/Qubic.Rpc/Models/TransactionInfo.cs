namespace Qubic.Rpc.Models;

/// <summary>
/// A transaction from the archive.
/// </summary>
public sealed class TransactionInfo
{
    public string Hash { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public string Amount { get; set; } = "0";
    public uint TickNumber { get; set; }
    public string Timestamp { get; set; } = string.Empty;
    public uint InputType { get; set; }
    public uint InputSize { get; set; }
    public string InputData { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
    public bool? MoneyFlew { get; set; }
}
