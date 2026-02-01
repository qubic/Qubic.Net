namespace Qubic.Core.Entities;

/// <summary>
/// Represents the balance information for a Qubic identity.
/// </summary>
public sealed class QubicBalance
{
    /// <summary>
    /// The identity this balance belongs to.
    /// </summary>
    public required QubicIdentity Identity { get; init; }

    /// <summary>
    /// The current balance in QU (smallest unit).
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// The number of incoming transfers.
    /// </summary>
    public uint IncomingCount { get; init; }

    /// <summary>
    /// The number of outgoing transfers.
    /// </summary>
    public uint OutgoingCount { get; init; }

    /// <summary>
    /// The tick number at which this balance was queried.
    /// </summary>
    public uint? AtTick { get; init; }
}
