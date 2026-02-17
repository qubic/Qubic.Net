using Qubic.Core.Abstractions;
using Qubic.Core.Entities;
using Qubic.Core.Payloads;
using Qubic.Crypto;

namespace Qubic.Core;

/// <summary>
/// Default implementation of ITransactionBuilder.
/// </summary>
public sealed class TransactionBuilder : ITransactionBuilder
{
    private readonly IQubicCrypt _crypt;

    public TransactionBuilder() : this(new QubicCrypt())
    {
    }

    public TransactionBuilder(IQubicCrypt crypt)
    {
        _crypt = crypt ?? throw new ArgumentNullException(nameof(crypt));
    }

    public QubicTransaction CreateTransfer(
        QubicIdentity source,
        QubicIdentity destination,
        long amount,
        uint tick)
    {
        return CreateTransaction(source, destination, amount, tick, new TransferPayload());
    }

    public QubicTransaction CreateTransaction(
        QubicIdentity source,
        QubicIdentity destination,
        long amount,
        uint tick,
        ITransactionPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        return new QubicTransaction
        {
            SourceIdentity = source,
            DestinationIdentity = destination,
            Amount = amount,
            Tick = tick,
            InputType = payload.InputType,
            InputSize = payload.InputSize,
            Payload = payload.GetPayloadBytes()
        };
    }

    public void Sign(QubicTransaction transaction, string seed)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(seed);

        if (seed.Length != 55)
            throw new ArgumentException("Seed must be 55 characters.", nameof(seed));

        if (transaction.IsSigned)
            throw new InvalidOperationException("Transaction is already signed.");

        // Build the unsigned transaction bytes
        var unsignedBytes = BuildUnsignedTransactionBytes(transaction);

        // Sign the transaction
        var signature = _crypt.Sign(seed, unsignedBytes);

        // Compute transaction hash (K12 of unsigned bytes + signature)
        var signedBytes = new byte[unsignedBytes.Length + 64];
        unsignedBytes.CopyTo(signedBytes, 0);
        signature.CopyTo(signedBytes, unsignedBytes.Length);
        var hash = _crypt.GetHumanReadableBytes(_crypt.KangarooTwelve(signedBytes));

        transaction.SetSignature(signature, hash);
    }

    private static byte[] BuildUnsignedTransactionBytes(QubicTransaction transaction)
    {
        // Transaction structure:
        // - Source public key: 32 bytes
        // - Destination public key: 32 bytes
        // - Amount: 8 bytes
        // - Tick: 4 bytes
        // - Input type: 2 bytes
        // - Input size: 2 bytes
        // - Payload: variable

        var payloadSize = transaction.Payload?.Length ?? 0;
        var totalSize = 32 + 32 + 8 + 4 + 2 + 2 + payloadSize;
        var bytes = new byte[totalSize];
        var offset = 0;

        // Source public key
        Array.Copy(transaction.SourceIdentity.PublicKey, 0, bytes, offset, 32);
        offset += 32;

        // Destination public key
        Array.Copy(transaction.DestinationIdentity.PublicKey, 0, bytes, offset, 32);
        offset += 32;

        // Amount (little-endian)
        BitConverter.TryWriteBytes(bytes.AsSpan(offset, 8), transaction.Amount);
        offset += 8;

        // Tick (little-endian)
        BitConverter.TryWriteBytes(bytes.AsSpan(offset, 4), transaction.Tick);
        offset += 4;

        // Input type (little-endian)
        BitConverter.TryWriteBytes(bytes.AsSpan(offset, 2), transaction.InputType);
        offset += 2;

        // Input size (little-endian)
        BitConverter.TryWriteBytes(bytes.AsSpan(offset, 2), transaction.InputSize);
        offset += 2;

        // Payload
        if (transaction.Payload is not null && transaction.Payload.Length > 0)
        {
            Array.Copy(transaction.Payload, 0, bytes, offset, transaction.Payload.Length);
        }

        return bytes;
    }
}
