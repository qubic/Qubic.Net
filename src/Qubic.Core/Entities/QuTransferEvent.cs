namespace Qubic.Core.Entities;

/// <summary>
/// Represents a QU transfer event (logged on-chain).
/// </summary>
public sealed class QuTransferEvent
{
    /// <summary>
    /// The source public key (32 bytes).
    /// </summary>
    public required byte[] SourcePublicKey { get; init; }

    /// <summary>
    /// The destination public key (32 bytes).
    /// </summary>
    public required byte[] DestinationPublicKey { get; init; }

    /// <summary>
    /// The amount transferred.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// Gets the source identity.
    /// </summary>
    public QubicIdentity Source => QubicIdentity.FromPublicKey(SourcePublicKey);

    /// <summary>
    /// Gets the destination identity.
    /// </summary>
    public QubicIdentity Destination => QubicIdentity.FromPublicKey(DestinationPublicKey);
}
