using System;
using System.Linq;

namespace Qubic.Crypto;

/// <summary>
/// Interface for Qubic cryptographic operations.
/// </summary>
public interface IQubicCrypt
{
    /// <summary>
    /// Computes KangarooTwelve hash of input data.
    /// </summary>
    byte[] KangarooTwelve(byte[] inputData, int outputByteLen, int? fixedInputDataLength = null);

    /// <summary>
    /// Computes KangarooTwelve hash with 32-byte output.
    /// </summary>
    byte[] KangarooTwelve(byte[] inputData, int? fixedInputDataLength = null);

    /// <summary>
    /// Computes KangarooTwelve hash of 64 bytes to 32 bytes output.
    /// </summary>
    byte[] KangarooTwelve64To32(byte[] inputData);

    /// <summary>
    /// Computes ECDH shared key using seed and peer's public key.
    /// </summary>
    byte[] GetSharedKey(string seed, byte[] publicKey);

    /// <summary>
    /// Computes ECDH shared key using private key and peer's public key.
    /// </summary>
    byte[] GetSharedKey(byte[] privateKey, byte[] publicKey);

    /// <summary>
    /// Gets the 32-byte private key from a 55-character seed.
    /// </summary>
    byte[] GetPrivateKey(string seed);

    /// <summary>
    /// Gets the 32-byte public key from a 55-character seed.
    /// </summary>
    byte[] GetPublicKey(string seed);

    /// <summary>
    /// Converts a 32-byte public key to a 60-character Qubic identity.
    /// </summary>
    string GetIdentityFromPublicKey(byte[] publicKey);

    /// <summary>
    /// Converts bytes to human-readable format (shifted hex using a-p).
    /// </summary>
    string GetHumanReadableBytes(byte[] data);

    /// <summary>
    /// Signs a message using the seed. Returns message with signature appended.
    /// </summary>
    byte[] Sign(string seed, byte[] message);

    /// <summary>
    /// Verifies a message where signature is the last 64 bytes.
    /// </summary>
    bool Verify(byte[] publicKey, byte[] message);

    /// <summary>
    /// Verifies a message with separate signature.
    /// </summary>
    bool Verify(byte[] publicKey, byte[] message, byte[] signature);

    /// <summary>
    /// Converts shifted hex string (a-p) to bytes.
    /// </summary>
    byte[] ShiftedHexToBytes(string hex)
    {
        const int HEX_CHARS_PER_BYTE = 2;
        const int HEX_BASE = 16;
        const string SHIFTED_HEX_CHARS = "abcdefghijklmnop";
        const string HEX_CHARS = "0123456789abcdef";

        hex = hex.ToLower();

        if (hex.Length % HEX_CHARS_PER_BYTE != 0)
        {
            hex = "a" + hex;
        }

        byte[] bytes = new byte[hex.Length / HEX_CHARS_PER_BYTE];

        for (int i = 0, c = 0; c < hex.Length; c += HEX_CHARS_PER_BYTE)
        {
            string hexChunk = hex.Substring(c, HEX_CHARS_PER_BYTE);
            string processedHex = new string(hexChunk.Select(ch => HEX_CHARS[SHIFTED_HEX_CHARS.IndexOf(ch)]).ToArray());
            bytes[i++] = Convert.ToByte(processedHex, HEX_BASE);
        }

        return bytes;
    }

    /// <summary>
    /// Converts human-readable string (e.g., txid) to digest bytes.
    /// </summary>
    byte[] GetDigestFromHumanReadableString(string humanReadableString)
    {
        if (string.IsNullOrEmpty(humanReadableString))
            throw new ArgumentException("humanReadableString must not be null");
        return GetPublicKeyFromIdentity(humanReadableString.ToUpper());
    }

    /// <summary>
    /// Converts a 60-character identity to 32-byte public key.
    /// Last 4 characters are checksum (ignored).
    /// </summary>
    byte[] GetPublicKeyFromIdentity(string identity)
    {
        if (identity == null) throw new ArgumentException("Identity must not be null");
        if (identity.Length < 56) throw new ArgumentException("Identity must be 56-60 chars");
        if (identity.Length > 60) throw new ArgumentException("Identity must be 56-60 chars");

        byte[] buffer = new byte[32];

        for (int i = 0; i < 4; i++)
        {
            ulong value = 0;

            for (int j = 13; j >= 0; j--)
            {
                char c = identity[i * 14 + j];
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException("Invalid Identity [A-Z]");

                value = value * 26UL + (ulong)(c - 'A');
            }

            int offset = i * 8;
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            buffer[offset + 4] = (byte)(value >> 32);
            buffer[offset + 5] = (byte)(value >> 40);
            buffer[offset + 6] = (byte)(value >> 48);
            buffer[offset + 7] = (byte)(value >> 56);
        }

        return buffer;
    }
}
