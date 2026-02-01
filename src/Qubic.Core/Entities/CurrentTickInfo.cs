namespace Qubic.Core.Entities;

/// <summary>
/// Current tick info response from the network.
/// </summary>
public sealed class CurrentTickInfo
{
    /// <summary>
    /// Target tick duration in milliseconds.
    /// </summary>
    public required ushort TickDuration { get; init; }

    /// <summary>
    /// Current epoch.
    /// </summary>
    public required ushort Epoch { get; init; }

    /// <summary>
    /// Current tick number.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// Number of computors with aligned votes.
    /// </summary>
    public required ushort NumberOfAlignedVotes { get; init; }

    /// <summary>
    /// Number of computors with misaligned votes.
    /// </summary>
    public required ushort NumberOfMisalignedVotes { get; init; }

    /// <summary>
    /// Initial tick of the current epoch.
    /// </summary>
    public required uint InitialTick { get; init; }
}
