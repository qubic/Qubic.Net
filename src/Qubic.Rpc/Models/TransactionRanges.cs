namespace Qubic.Rpc.Models;

public sealed class TransactionRanges
{
    public RangeFilter? Amount { get; set; }
    public RangeFilter? TickNumber { get; set; }
    public RangeFilter? InputType { get; set; }
    public RangeFilter? Timestamp { get; set; }
}
