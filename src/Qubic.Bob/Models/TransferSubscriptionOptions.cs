namespace Qubic.Bob.Models;

/// <summary>
/// Options for a transfers subscription.
/// </summary>
public sealed class TransferSubscriptionOptions
{
    /// <summary>
    /// Identities to monitor for transfers.
    /// </summary>
    public List<string>? Identities { get; set; }

    /// <summary>
    /// Log ID to start catching up from.
    /// </summary>
    public long? StartLogId { get; set; }

    /// <summary>
    /// Epoch to start catching up from.
    /// </summary>
    public uint? StartEpoch { get; set; }
}
