using System;
using System.Numerics;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Prime field arithmetic over p = 2^127 - 1 (Mersenne prime).
/// </summary>
public readonly struct Fp : IEquatable<Fp>
{
    /// <summary>
    /// The prime modulus p = 2^127 - 1
    /// </summary>
    public static readonly BigInteger P = (BigInteger.One << 127) - 1;

    /// <summary>
    /// The underlying value (always reduced mod p)
    /// </summary>
    public readonly BigInteger Value;

    public static readonly Fp Zero = new(BigInteger.Zero);
    public static readonly Fp One = new(BigInteger.One);

    public Fp(BigInteger value)
    {
        Value = Mod(value);
    }

    private static BigInteger Mod(BigInteger a)
    {
        // Fast modular reduction for Mersenne prime
        // a mod (2^127 - 1) = (a & mask) + (a >> 127) where mask = 2^127 - 1
        // May need to reduce again if result >= p
        var result = a;
        while (result < 0)
            result += P;
        while (result > P)
        {
            var high = result >> 127;
            var low = result & P;
            result = high + low;
        }
        if (result == P)
            result = BigInteger.Zero;
        return result;
    }

    public static Fp operator +(Fp a, Fp b)
    {
        return new Fp(a.Value + b.Value);
    }

    public static Fp operator -(Fp a, Fp b)
    {
        var diff = a.Value - b.Value;
        return new Fp(diff < 0 ? diff + P : diff);
    }

    public static Fp operator -(Fp a)
    {
        return a.Value == 0 ? Zero : new Fp(P - a.Value);
    }

    public static Fp operator *(Fp a, Fp b)
    {
        return new Fp(a.Value * b.Value);
    }

    public Fp Square()
    {
        return new Fp(Value * Value);
    }

    public Fp Div2()
    {
        // Division by 2: if even, just shift; if odd, add p first
        if (Value.IsEven)
            return new Fp(Value >> 1);
        else
            return new Fp((Value + P) >> 1);
    }

    /// <summary>
    /// Computes the multiplicative inverse using Fermat's little theorem: a^(-1) = a^(p-2) mod p
    /// </summary>
    public Fp Inverse()
    {
        if (Value == 0)
            throw new DivideByZeroException("Cannot invert zero");

        return Pow(P - 2);
    }

    /// <summary>
    /// Computes a^exp mod p using binary exponentiation.
    /// </summary>
    public Fp Pow(BigInteger exp)
    {
        var result = One;
        var baseVal = this;

        while (exp > 0)
        {
            if (!exp.IsEven)
                result *= baseVal;
            baseVal = baseVal.Square();
            exp >>= 1;
        }

        return result;
    }

    /// <summary>
    /// Computes the square root if it exists.
    /// For p ≡ 3 (mod 4), sqrt(a) = a^((p+1)/4)
    /// </summary>
    public Fp? Sqrt()
    {
        if (Value == 0)
            return Zero;

        // p = 2^127 - 1 ≡ 3 (mod 4), so sqrt = a^((p+1)/4)
        var exp = (P + 1) >> 2;
        var candidate = Pow(exp);

        // Verify
        if (candidate.Square().Value == Value)
            return candidate;

        return null;
    }

    /// <summary>
    /// Creates an Fp from two 64-bit values (little-endian).
    /// </summary>
    public static Fp FromU64LE(ulong lo, ulong hi)
    {
        // Mask to 127 bits (high bit of hi must be 0 for valid Fp)
        var hiMasked = hi & 0x7FFFFFFFFFFFFFFFUL;
        var value = new BigInteger(lo) | (new BigInteger(hiMasked) << 64);
        return new Fp(value);
    }

    /// <summary>
    /// Converts to 16 bytes (little-endian).
    /// </summary>
    public void ToBytes(Span<byte> output)
    {
        if (output.Length < 16)
            throw new ArgumentException("Output must be at least 16 bytes", nameof(output));

        var bytes = Value.ToByteArray(isUnsigned: true, isBigEndian: false);
        bytes.AsSpan().CopyTo(output);
        // Zero-pad if needed
        for (int i = bytes.Length; i < 16; i++)
            output[i] = 0;
    }

    /// <summary>
    /// Creates an Fp from 16 bytes (little-endian).
    /// </summary>
    public static Fp FromBytes(ReadOnlySpan<byte> input)
    {
        if (input.Length < 16)
            throw new ArgumentException("Input must be at least 16 bytes", nameof(input));

        var bytes = new byte[17]; // Extra byte for unsigned
        input.Slice(0, 16).CopyTo(bytes);
        bytes[16] = 0; // Ensure unsigned
        var value = new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        return new Fp(value);
    }

    public bool Equals(Fp other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is Fp other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public static bool operator ==(Fp left, Fp right) => left.Equals(right);
    public static bool operator !=(Fp left, Fp right) => !left.Equals(right);
    public override string ToString() => Value.ToString("X");

    public bool IsZero => Value == 0;
}
