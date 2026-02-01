namespace Qubic.Core.Entities;

/// <summary>
/// Represents an entity (account) record in the Qubic spectrum.
/// This is the on-chain state for an identity.
/// Size: 64 bytes
/// </summary>
public sealed class EntityRecord
{
    /// <summary>
    /// The entity's public key (32 bytes).
    /// </summary>
    public required byte[] PublicKey { get; init; }

    /// <summary>
    /// Total incoming amount received by this entity.
    /// </summary>
    public required long IncomingAmount { get; init; }

    /// <summary>
    /// Total outgoing amount sent by this entity.
    /// </summary>
    public required long OutgoingAmount { get; init; }

    /// <summary>
    /// Number of incoming transfers to this entity.
    /// </summary>
    public required uint NumberOfIncomingTransfers { get; init; }

    /// <summary>
    /// Number of outgoing transfers from this entity.
    /// </summary>
    public required uint NumberOfOutgoingTransfers { get; init; }

    /// <summary>
    /// Tick number of the latest incoming transfer.
    /// </summary>
    public required uint LatestIncomingTransferTick { get; init; }

    /// <summary>
    /// Tick number of the latest outgoing transfer.
    /// </summary>
    public required uint LatestOutgoingTransferTick { get; init; }

    /// <summary>
    /// Gets the current balance (incoming - outgoing).
    /// </summary>
    public long Balance => IncomingAmount - OutgoingAmount;

    /// <summary>
    /// Gets the identity for this entity.
    /// </summary>
    public QubicIdentity Identity => QubicIdentity.FromPublicKey(PublicKey);
}
