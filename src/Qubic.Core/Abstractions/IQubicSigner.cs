namespace Qubic.Core.Abstractions;

/// <summary>
/// Interface for signing data with a Qubic identity.
/// </summary>
public interface IQubicSigner
{
    /// <summary>
    /// Signs the given message digest.
    /// </summary>
    /// <param name="messageDigest">The 32-byte message digest to sign.</param>
    /// <returns>The 64-byte signature.</returns>
    byte[] Sign(byte[] messageDigest);

    /// <summary>
    /// Gets the public key of the signer.
    /// </summary>
    byte[] PublicKey { get; }
}
