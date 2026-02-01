namespace Qubic.Core.Payloads;

/// <summary>
/// Payload for a simple QU transfer (no payload data, InputType = 0).
/// </summary>
public sealed class TransferPayload : ITransactionPayload
{
    public ushort InputType => 0;
    public ushort InputSize => 0;
    public byte[] GetPayloadBytes() => [];
}
