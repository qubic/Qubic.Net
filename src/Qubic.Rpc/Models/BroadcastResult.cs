namespace Qubic.Rpc.Models;

/// <summary>
/// Result of broadcasting a transaction.
/// </summary>
public sealed class BroadcastResult
{
    public string TransactionId { get; set; } = string.Empty;
    public int PeersBroadcasted { get; set; }
}
