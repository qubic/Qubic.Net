using System;
using Qubic.Crypto;

namespace Qubic.Core;

/// <summary>
/// Computes the spectrum (accounts) Merkle tree digest.
/// The spectrum is a flat array of entity records; this class builds a binary
/// Merkle tree over those records using KangarooTwelve (K12) hashing.
/// </summary>
public static class SpectrumDigest
{
    /// <summary>
    /// Computes the spectrum Merkle tree root digest from raw spectrum data.
    /// </summary>
    /// <param name="spectrumData">
    /// Raw spectrum data: <see cref="QubicConstants.SpectrumCapacity"/> ×
    /// <see cref="QubicConstants.EntityRecordSize"/> bytes (≈1 GB).
    /// </param>
    /// <returns>32-byte Merkle root digest.</returns>
    public static byte[] ComputeDigest(byte[] spectrumData)
    {
        if (spectrumData == null)
            throw new ArgumentNullException(nameof(spectrumData));

        long expectedSize = (long)QubicConstants.SpectrumCapacity * QubicConstants.EntityRecordSize;
        if (spectrumData.Length != expectedSize)
            throw new ArgumentException(
                $"Spectrum data must be {expectedSize} bytes ({QubicConstants.SpectrumCapacity} x {QubicConstants.EntityRecordSize})",
                nameof(spectrumData));

        // Level 0: hash each entity record (64 bytes) → 32-byte leaf digest
        var currentLevel = new byte[QubicConstants.SpectrumCapacity][];
        for (int i = 0; i < QubicConstants.SpectrumCapacity; i++)
        {
            var entitySpan = spectrumData.AsSpan(
                i * QubicConstants.EntityRecordSize,
                QubicConstants.EntityRecordSize);
            currentLevel[i] = K12.Hash(entitySpan, QubicConstants.DigestSize);
        }

        // Build Merkle tree level by level, pairing siblings
        var combined = new byte[QubicConstants.DigestSize * 2]; // reusable 64-byte buffer
        while (currentLevel.Length > 1)
        {
            var nextLevel = new byte[currentLevel.Length / 2][];
            for (int i = 0; i < currentLevel.Length; i += 2)
            {
                Buffer.BlockCopy(currentLevel[i], 0, combined, 0, QubicConstants.DigestSize);
                Buffer.BlockCopy(currentLevel[i + 1], 0, combined, QubicConstants.DigestSize, QubicConstants.DigestSize);
                nextLevel[i / 2] = K12.Hash(combined, QubicConstants.DigestSize);
            }
            currentLevel = nextLevel;
        }

        return currentLevel[0];
    }

    /// <summary>
    /// Computes the spectrum Merkle tree root digest and returns it as a
    /// 60-character Qubic identity string.
    /// </summary>
    public static string ComputeDigestAsIdentity(byte[] spectrumData)
    {
        var digest = ComputeDigest(spectrumData);
        var crypt = new QubicCrypt();
        return crypt.GetIdentityFromPublicKey(digest);
    }
}
