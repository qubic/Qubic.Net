namespace Qubic.Rpc.Models;

internal sealed class BalanceData
{
    public string Id { get; set; } = string.Empty;
    public string Balance { get; set; } = "0";
    public uint ValidForTick { get; set; }
    public uint LatestIncomingTransferTick { get; set; }
    public uint LatestOutgoingTransferTick { get; set; }
    public string IncomingAmount { get; set; } = "0";
    public string OutgoingAmount { get; set; } = "0";
    public uint NumberOfIncomingTransfers { get; set; }
    public uint NumberOfOutgoingTransfers { get; set; }
}
