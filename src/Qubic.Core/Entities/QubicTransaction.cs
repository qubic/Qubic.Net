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
    /// Returns the raw transaction bytes (src + dst + amount + tick + inputType + inputSize + payload + signature).
    /// Transaction must be signed.
    /// </summary>
    public byte[] GetRawBytes()
    {
        if (!IsSigned)
            throw new InvalidOperationException("Transaction must be signed to get raw bytes.");

        var payloadSize = Payload?.Length ?? 0;
        var totalSize = 32 + 32 + 8 + 4 + 2 + 2 + payloadSize + 64;
        var bytes = new byte[totalSize];
        var offset = 0;

        Array.Copy(SourceIdentity.PublicKey, 0, bytes, offset, 32);
        offset += 32;
        Array.Copy(DestinationIdentity.PublicKey, 0, bytes, offset, 32);
        offset += 32;
        System.Buffers.Binary.BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(offset), Amount);
        offset += 8;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset), Tick);
        offset += 4;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), InputType);
        offset += 2;
        System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), InputSize);
        offset += 2;
        if (Payload is not null && Payload.Length > 0)
        {
            Array.Copy(Payload, 0, bytes, offset, Payload.Length);
            offset += Payload.Length;
        }
        Array.Copy(Signature!, 0, bytes, offset, 64);

        return bytes;
    }

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
