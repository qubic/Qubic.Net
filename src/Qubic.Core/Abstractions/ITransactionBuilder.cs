using Qubic.Core.Entities;
using Qubic.Core.Payloads;

namespace Qubic.Core.Abstractions;

/// <summary>
/// Interface for building and signing Qubic transactions.
/// </summary>
public interface ITransactionBuilder
{
    /// <summary>
    /// Creates a simple QU transfer transaction.
    /// </summary>
    /// <param name="source">The source identity.</param>
    /// <param name="destination">The destination identity.</param>
    /// <param name="amount">The amount of QU to transfer.</param>
    /// <param name="tick">The target tick for execution.</param>
    QubicTransaction CreateTransfer(
        QubicIdentity source,
        QubicIdentity destination,
        long amount,
        uint tick);

    /// <summary>
    /// Creates a transaction with a custom payload.
    /// </summary>
    /// <param name="source">The source identity.</param>
    /// <param name="destination">The destination identity (or contract address).</param>
    /// <param name="amount">The amount of QU to transfer.</param>
    /// <param name="tick">The target tick for execution.</param>
    /// <param name="payload">The transaction payload.</param>
    QubicTransaction CreateTransaction(
        QubicIdentity source,
        QubicIdentity destination,
        long amount,
        uint tick,
        ITransactionPayload payload);

    /// <summary>
    /// Signs a transaction with the given seed.
    /// </summary>
    /// <param name="transaction">The transaction to sign.</param>
    /// <param name="seed">The 55-character seed of the source identity.</param>
    void Sign(QubicTransaction transaction, string seed);
}
