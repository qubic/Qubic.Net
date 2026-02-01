namespace Qubic.Core.Entities;

/// <summary>
/// Represents a Qubic transaction.
/// </summary>
public sealed class QubicTransaction
{
    /// <summary>
    /// The source identity sending the transaction.
    /// </summary>
    public required QubicIdentity SourceIdentity { get; init; }

    /// <summary>
    /// The destination identity receiving the transaction.
    /// </summary>
    public required QubicIdentity DestinationIdentity { get; init; }

    /// <summary>
    /// The amount of QU to transfer.
    /// </summary>
    public required long Amount { get; init; }

    /// <summary>
    /// The tick number when this transaction should be executed.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// The input type (0 for standard transfer, other values for smart contract calls).
    /// </summary>
    public ushort InputType { get; init; }

    /// <summary>
    /// The input size in bytes.
    /// </summary>
    public ushort InputSize { get; init; }

    /// <summary>
    /// Optional payload data for smart contract interactions.
    /// </summary>
    public byte[]? Payload { get; init; }

    /// <summary>
    /// The 64-byte signature of the transaction (set after signing).
    /// </summary>
    public byte[]? Signature { get; private set; }

    /// <summary>
    /// The transaction hash/digest (computed from the signed transaction).
    /// </summary>
    public string? TransactionHash { get; private set; }

    /// <summary>
    /// Whether this transaction has been signed.
    /// </summary>
    public bool IsSigned => Signature is not null;

    /// <summary>
    /// Sets the signature for this transaction.
    /// </summary>
    internal void SetSignature(byte[] signature, string hash)
    {
        ArgumentNullException.ThrowIfNull(signature);
        if (signature.Length != 64)
            throw new ArgumentException("Signature must be 64 bytes.", nameof(signature));

        Signature = signature;
        TransactionHash = hash;
    }
}
