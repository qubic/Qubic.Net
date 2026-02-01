namespace Qubic.Rpc.Models;

/// <summary>
/// Result of a transactions-for-identity query.
/// </summary>
public sealed class TransactionsForIdentityResult
{
    public uint ValidForTick { get; set; }
    public HitsInfo Hits { get; set; } = new();
    public List<TransactionInfo> Transactions { get; set; } = [];
}
