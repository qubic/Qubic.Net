namespace Qubic.Rpc.Models;

/// <summary>
/// Last processed tick and interval data from the archive.
/// </summary>
public sealed class LastProcessedTick
{
    public uint TickNumber { get; set; }
    public uint Epoch { get; set; }
    public uint IntervalInitialTick { get; set; }
}
