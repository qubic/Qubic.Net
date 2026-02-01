namespace Qubic.Rpc.Models;

/// <summary>
/// A range of processed ticks in the archive.
/// </summary>
public sealed class ProcessedTickInterval
{
    public uint Epoch { get; set; }
    public uint FirstTick { get; set; }
    public uint LastTick { get; set; }
}
