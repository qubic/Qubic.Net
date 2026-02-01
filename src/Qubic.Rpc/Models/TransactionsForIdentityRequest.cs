namespace Qubic.Rpc.Models;

/// <summary>
/// Request for querying transactions by identity with optional filtering.
/// </summary>
public sealed class TransactionsForIdentityRequest
{
    public required string Identity { get; set; }
    public TransactionFilters? Filters { get; set; }
    public TransactionRanges? Ranges { get; set; }
    public PaginationOptions? Pagination { get; set; }
}
