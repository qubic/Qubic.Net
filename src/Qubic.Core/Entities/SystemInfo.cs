namespace Qubic.Core.Entities;

/// <summary>
/// System state information.
/// </summary>
public sealed class SystemInfo
{
    /// <summary>
    /// Protocol version (major).
    /// </summary>
    public required short Version { get; init; }

    /// <summary>
    /// Current epoch.
    /// </summary>
    public required ushort Epoch { get; init; }

    /// <summary>
    /// Current tick.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// Initial tick of the current epoch.
    /// </summary>
    public required uint InitialTick { get; init; }

    /// <summary>
    /// Latest tick that has been created.
    /// </summary>
    public required uint LatestCreatedTick { get; init; }

    /// <summary>
    /// Latest tick that has been led (finalized).
    /// </summary>
    public required uint LatestLedTick { get; init; }

    /// <summary>
    /// Initial timestamp components for the epoch.
    /// </summary>
    public required DateTime InitialTimestamp { get; init; }

    /// <summary>
    /// Total number of valid solutions found.
    /// </summary>
    public required uint NumberOfSolutions { get; init; }
}
