namespace Qubic.Core.Entities;

/// <summary>
/// Represents a contract IPO (Initial Public Offering) state.
/// Each contract has 676 share slots corresponding to computors.
/// Size: 27,040 bytes (676 * 32 + 676 * 8)
/// </summary>
public sealed class ContractIpo
{
    /// <summary>
    /// Public keys of the IPO participants (676 slots).
    /// </summary>
    public required byte[][] PublicKeys { get; init; }

    /// <summary>
    /// Bid prices for each slot (676 slots).
    /// </summary>
    public required long[] Prices { get; init; }

    /// <summary>
    /// Gets the identity for a specific slot.
    /// </summary>
    public QubicIdentity? GetParticipantIdentity(int index)
    {
        if (index < 0 || index >= QubicConstants.NumberOfComputors)
            throw new ArgumentOutOfRangeException(nameof(index));

        var pubKey = PublicKeys[index];
        if (pubKey.All(b => b == 0))
            return null;

        return QubicIdentity.FromPublicKey(pubKey);
    }
}
