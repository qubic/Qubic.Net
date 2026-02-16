namespace Qubic.Services.Storage;

/// <summary>
/// Filter and pagination parameters for querying stored log events.
/// </summary>
public sealed class LogEventQuery
{
    public int? LogType { get; set; }
    /// <summary>
    /// Filter by contract index extracted from the JSON body (_contractIndex field).
    /// Only applicable for contract log types (4-7).
    /// </summary>
    public int? ContractIndex { get; set; }
    public uint? MinTick { get; set; }
    public uint? MaxTick { get; set; }
    public uint? Epoch { get; set; }
    public string? TxHash { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; } = 50;
}
