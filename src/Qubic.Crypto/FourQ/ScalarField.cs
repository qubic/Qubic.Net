using System;
using System.Numerics;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Scalar field arithmetic modulo the FourQ curve order.
/// </summary>
public static class ScalarField
{
    /// <summary>
    /// The order of the FourQ curve (number of points on the prime-order subgroup).
    /// From FourQlib: curve_order[4] = { 0x2FB2540EC7768CE7, 0xDFBD004DFE0F7999,
    ///                                   0xF05397829CBC14E5, 0x0029CBC14E5E0A72 }
    /// N = 0x29CBC14E5E0A72F05397829CBC14E5DFBD004DFE0F79992FB2540EC7768CE7
    /// </summary>
    public static readonly BigInteger N = BigInteger.Parse(
        "029CBC14E5E0A72F05397829CBC14E5DFBD004DFE0F79992FB2540EC7768CE7",
        System.Globalization.NumberStyles.HexNumber
    );

    /// <summary>
    /// Reduces a value modulo N.
    /// </summary>
    public static BigInteger Reduce(BigInteger value)
    {
        var result = value % N;
        if (result < 0)
            result += N;
        return result;
    }

    /// <summary>
    /// Converts 32 bytes (little-endian) to a scalar, reduced mod N.
    /// </summary>
    public static BigInteger FromBytes32LE(ReadOnlySpan<byte> input)
    {
        if (input.Length < 32)
            throw new ArgumentException("Input must be at least 32 bytes", nameof(input));

        var bytes = new byte[33];
        input.Slice(0, 32).CopyTo(bytes);
        bytes[32] = 0; // Ensure unsigned
        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        return Reduce(value);
    }

    /// <summary>
    /// Converts 64 bytes (little-endian) to a scalar, reduced mod N.
    /// Used for hash outputs that need wider reduction.
    /// </summary>
    public static BigInteger FromBytes64LE(ReadOnlySpan<byte> input)
    {
        if (input.Length < 64)
            throw new ArgumentException("Input must be at least 64 bytes", nameof(input));

        var bytes = new byte[65];
        input.Slice(0, 64).CopyTo(bytes);
        bytes[64] = 0; // Ensure unsigned
        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        return Reduce(value);
    }

    /// <summary>
    /// Converts a scalar to 32 bytes (little-endian).
    /// </summary>
    public static byte[] ToBytes32LE(BigInteger value)
    {
        var result = new byte[32];
        ToBytes32LE(value, result);
        return result;
    }

    /// <summary>
    /// Converts a scalar to 32 bytes (little-endian) into the provided buffer.
    /// </summary>
    public static void ToBytes32LE(BigInteger value, Span<byte> output)
    {
        if (output.Length < 32)
            throw new ArgumentException("Output must be at least 32 bytes", nameof(output));

        var reduced = Reduce(value);
        var bytes = reduced.ToByteArray(isUnsigned: true, isBigEndian: false);
        bytes.AsSpan().CopyTo(output);
        // Zero-pad if needed
        for (int i = bytes.Length; i < 32; i++)
            output[i] = 0;
    }

    /// <summary>
    /// Multiplies two scalars modulo N.
    /// </summary>
    public static BigInteger Mul(BigInteger a, BigInteger b)
    {
        return Reduce(a * b);
    }

    /// <summary>
    /// Adds two scalars modulo N.
    /// </summary>
    public static BigInteger Add(BigInteger a, BigInteger b)
    {
        return Reduce(a + b);
    }

    /// <summary>
    /// Subtracts two scalars modulo N.
    /// </summary>
    public static BigInteger Sub(BigInteger a, BigInteger b)
    {
        return Reduce(a - b);
    }

    /// <summary>
    /// Computes the modular inverse using extended Euclidean algorithm.
    /// </summary>
    public static BigInteger Inverse(BigInteger a)
    {
        return BigInteger.ModPow(a, N - 2, N);
    }
}
