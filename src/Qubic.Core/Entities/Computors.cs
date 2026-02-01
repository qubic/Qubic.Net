namespace Qubic.Core.Entities;

/// <summary>
/// Represents the set of computors for an epoch.
/// Size: 21,698 bytes (2 + 676*32 + 64)
/// </summary>
public sealed class Computors
{
    /// <summary>
    /// The epoch these computors are valid for.
    /// </summary>
    public required ushort Epoch { get; init; }

    /// <summary>
    /// The public keys of all 676 computors.
    /// </summary>
    public required byte[][] PublicKeys { get; init; }

    /// <summary>
    /// The signature validating this computor set.
    /// </summary>
    public required byte[] Signature { get; init; }

    /// <summary>
    /// Gets the identity for a specific computor index.
    /// </summary>
    public QubicIdentity GetComputorIdentity(int index)
    {
        if (index < 0 || index >= QubicConstants.NumberOfComputors)
            throw new ArgumentOutOfRangeException(nameof(index));

        return QubicIdentity.FromPublicKey(PublicKeys[index]);
    }
}
