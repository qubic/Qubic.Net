namespace Qubic.Rpc.Models;

public sealed class TransactionFilters
{
    public string? Source { get; set; }
    public string? Destination { get; set; }
    public string? Amount { get; set; }
    public string? InputType { get; set; }
}
