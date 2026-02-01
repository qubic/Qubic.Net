namespace Qubic.Core.Payloads;

/// <summary>
/// Interface for transaction payloads.
/// </summary>
public interface ITransactionPayload
{
    /// <summary>
    /// The input type identifier for this payload.
    /// </summary>
    ushort InputType { get; }

    /// <summary>
    /// The size of the payload data in bytes.
    /// </summary>
    ushort InputSize { get; }

    /// <summary>
    /// Gets the serialized payload bytes.
    /// </summary>
    byte[] GetPayloadBytes();
}
