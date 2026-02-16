namespace Qubic.Services.Storage;

public enum TransactionDirection { All, Sent, Received }
public enum TransactionSortOrder { TickDesc, TickAsc }
public enum TxHashType { All, User, System }

/// <summary>
/// Filter and pagination parameters for querying stored transactions.
/// </summary>
public sealed class TransactionQuery
{
    public TransactionDirection Direction { get; set; } = TransactionDirection.All;
    public TxHashType HashType { get; set; } = TxHashType.All;
    public uint? MinTick { get; set; }
    public uint? MaxTick { get; set; }
    public uint? InputType { get; set; }
    /// <summary>Filter by destination identity (exact match).</summary>
    public string? Destination { get; set; }
    public string? SearchHash { get; set; }
    public TransactionSortOrder SortOrder { get; set; } = TransactionSortOrder.TickDesc;
    public int Offset { get; set; }
    public int Limit { get; set; } = 50;
}
