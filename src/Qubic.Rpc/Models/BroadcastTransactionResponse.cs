namespace Qubic.Rpc.Models;

internal sealed class BroadcastTransactionResponse
{
    public int PeersBroadcasted { get; set; }
    public string EncodedTransaction { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
}
