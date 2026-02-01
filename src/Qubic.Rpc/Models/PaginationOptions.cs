namespace Qubic.Rpc.Models;

public sealed class PaginationOptions
{
    public uint Offset { get; set; }
    public uint Size { get; set; } = 10;
}
