namespace Qubic.Rpc.Models;

internal sealed class TickInfoData
{
    public uint Tick { get; set; }
    public uint Duration { get; set; }
    public uint Epoch { get; set; }
    public uint InitialTick { get; set; }
}
