using Qubic.Core.Abstractions;
using Qubic.Crypto;

namespace Qubic.Core;

/// <summary>
/// A signer implementation that uses a seed for signing operations.
/// </summary>
public sealed class SeedSigner : IQubicSigner
{
    private readonly string _seed;
    private readonly IQubicCrypt _crypt;
    private readonly byte[] _publicKey;

    public SeedSigner(string seed) : this(seed, new QubicCrypt())
    {
    }

    public SeedSigner(string seed, IQubicCrypt crypt)
    {
        ArgumentNullException.ThrowIfNull(seed);
        ArgumentNullException.ThrowIfNull(crypt);

        if (seed.Length != 55)
            throw new ArgumentException("Seed must be 55 characters.", nameof(seed));

        _seed = seed;
        _crypt = crypt;
        _publicKey = _crypt.GetPublicKey(seed);
    }

    public byte[] PublicKey => _publicKey;

    public byte[] Sign(byte[] messageDigest)
    {
        ArgumentNullException.ThrowIfNull(messageDigest);

        var signedMessage = _crypt.Sign(_seed, messageDigest);

        // Extract just the signature (last 64 bytes)
        var signature = new byte[64];
        Array.Copy(signedMessage, signedMessage.Length - 64, signature, 0, 64);
        return signature;
    }
}
