namespace Qubic.Bob.Models;

/// <summary>
/// Options for a logs subscription.
/// </summary>
public sealed class LogSubscriptionOptions
{
    /// <summary>
    /// Identities to monitor for log events.
    /// </summary>
    public List<string>? Identities { get; set; }

    /// <summary>
    /// Log types to filter by.
    /// </summary>
    public List<int>? LogTypes { get; set; }

    /// <summary>
    /// Log ID to start catching up from.
    /// </summary>
    public long? StartLogId { get; set; }

    /// <summary>
    /// Epoch to start catching up from.
    /// </summary>
    public uint? StartEpoch { get; set; }
}
