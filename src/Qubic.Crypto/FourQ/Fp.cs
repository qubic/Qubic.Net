using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Prime field arithmetic over p = 2^127 - 1 (Mersenne prime).
/// Uses UInt128 internally for allocation-free, fixed-width arithmetic.
/// </summary>
public readonly struct Fp : IEquatable<Fp>
{
    /// <summary>
    /// The prime modulus p = 2^127 - 1 (as BigInteger, for backward compatibility)
    /// </summary>
    public static readonly BigInteger P = (BigInteger.One << 127) - 1;

    /// <summary>
    /// The prime modulus as UInt128 mask
    /// </summary>
    private static readonly UInt128 PMask = (UInt128.One << 127) - 1;

    /// <summary>
    /// The underlying value (always reduced mod p, in [0, p))
    /// </summary>
    private readonly UInt128 _value;

    /// <summary>
    /// The underlying value as BigInteger (for backward compatibility).
    /// Prefer RawValue for performance-sensitive code.
    /// </summary>
    public BigInteger Value
    {
        get
        {
            Span<byte> bytes = stackalloc byte[17];
            BinaryPrimitives.WriteUInt64LittleEndian(bytes, (ulong)_value);
            BinaryPrimitives.WriteUInt64LittleEndian(bytes.Slice(8), (ulong)(_value >> 64));
            bytes[16] = 0; // unsigned
            return new BigInteger(bytes, isUnsigned: true, isBigEndian: false);
        }
    }

    /// <summary>
    /// The underlying value as UInt128 (zero-allocation access).
    /// </summary>
    public UInt128 RawValue => _value;

    public static readonly Fp Zero = default;
    public static readonly Fp One = new((UInt128)1);

    /// <summary>
    /// Precomputed exponent for sqrt: (p+1)/4 = 2^125
    /// </summary>
    private static readonly UInt128 SqrtExp = UInt128.One << 125;

    /// <summary>
    /// Precomputed exponent for inverse: p-2 = 2^127 - 3
    /// </summary>
    private static readonly UInt128 InvExp = PMask - 2;

    /// <summary>
    /// Creates an Fp from a BigInteger value (reduced mod p).
    /// </summary>
    public Fp(BigInteger value)
    {
        // Handle negative values
        while (value < 0)
            value += P;

        // Reduce mod p using BigInteger, then convert to UInt128
        while (value > P)
        {
            var high = value >> 127;
            var low = value & P;
            value = high + low;
        }
        if (value == P)
            value = BigInteger.Zero;

        // Convert to UInt128
        var bytes = value.ToByteArray(isUnsigned: true, isBigEndian: false);
        ulong lo = 0, hi = 0;
        if (bytes.Length >= 8)
        {
            lo = BitConverter.ToUInt64(bytes, 0);
            if (bytes.Length >= 16)
                hi = BitConverter.ToUInt64(bytes, 8);
            else if (bytes.Length > 8)
            {
                Span<byte> padded = stackalloc byte[8];
                bytes.AsSpan(8).CopyTo(padded);
                hi = BitConverter.ToUInt64(padded);
            }
        }
        else if (bytes.Length > 0)
        {
            Span<byte> padded = stackalloc byte[8];
            bytes.AsSpan().CopyTo(padded);
            lo = BitConverter.ToUInt64(padded);
        }

        _value = ((UInt128)hi << 64) | lo;
    }

    /// <summary>
    /// Creates an Fp from a UInt128 value (reduced mod p).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Fp(UInt128 value)
    {
        _value = Reduce(value);
    }

    /// <summary>
    /// Creates an Fp from a pre-reduced UInt128 value (no reduction needed).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Fp(UInt128 value, bool _)
    {
        _value = value;
    }

    /// <summary>
    /// Mersenne prime reduction: x mod (2^127 - 1)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Reduce(UInt128 value)
    {
        // x mod (2^127 - 1) = (x >> 127) + (x & mask)
        // At most 2 iterations needed for UInt128 input
        UInt128 lo = value & PMask;
        UInt128 hi = value >> 127;
        value = lo + hi;
        // One more pass if still >= p
        if (value >= PMask)
        {
            lo = value & PMask;
            hi = value >> 127;
            value = lo + hi;
        }
        if (value == PMask)
            value = 0;
        return value;
    }

    /// <summary>
    /// Reduces a 256-bit product (hi:lo) mod p = 2^127 - 1.
    /// Since 2^127 = 1 (mod p), we fold higher bits down.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static UInt128 Reduce256(UInt128 lo, UInt128 hi)
    {
        // 256-bit value = hi * 2^128 + lo
        // Split at bit 127:
        //   chunk0 = bits 0..126 = lo & PMask
        //   bits 127+ = (lo >> 127) | (hi << 1)
        // Since 2^127 = 1 (mod p): result = chunk0 + upper_bits

        UInt128 chunk0 = lo & PMask;
        // upper = bits 127..254 of the 256-bit value
        // lo >> 127 gives 1 bit (bit 127 of lo)
        // hi << 1 shifts hi left by 1 (safe since hi < 2^127 for inputs < 2^127)
        UInt128 upper = (lo >> 127) | (hi << 1);

        // upper might be up to ~128 bits, so reduce it too
        UInt128 upper_lo = upper & PMask;
        UInt128 upper_hi = upper >> 127;

        UInt128 result = chunk0 + upper_lo + upper_hi;

        // Final reductions (at most 2 passes)
        if (result >= PMask)
        {
            UInt128 r_lo = result & PMask;
            UInt128 r_hi = result >> 127;
            result = r_lo + r_hi;
        }
        if (result >= PMask)
        {
            UInt128 r_lo = result & PMask;
            UInt128 r_hi = result >> 127;
            result = r_lo + r_hi;
        }
        if (result == PMask)
            result = 0;

        return result;
    }

    /// <summary>
    /// Computes 128x128 -> 256-bit unsigned multiplication.
    /// Returns (lo, hi) where result = hi * 2^128 + lo.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Mul128(UInt128 a, UInt128 b, out UInt128 lo, out UInt128 hi)
    {
        ulong a0 = (ulong)a;
        ulong a1 = (ulong)(a >> 64);
        ulong b0 = (ulong)b;
        ulong b1 = (ulong)(b >> 64);

        // Four partial products (each fits in UInt128)
        UInt128 p00 = (UInt128)a0 * b0;
        UInt128 p01 = (UInt128)a0 * b1;
        UInt128 p10 = (UInt128)a1 * b0;
        UInt128 p11 = (UInt128)a1 * b1;

        // Combine: result = p11*2^128 + (p01+p10)*2^64 + p00
        UInt128 mid = p01 + p10;
        ulong midCarry = (mid < p01) ? 1u : 0u; // overflow from p01+p10

        lo = p00 + (mid << 64);
        ulong loCarry = (lo < p00) ? 1u : 0u; // overflow from adding mid<<64 to p00

        hi = p11 + (mid >> 64) + ((UInt128)midCarry << 64) + loCarry;
    }

    /// <summary>
    /// Computes 128-bit squaring -> 256-bit result.
    /// Optimized: only 3 partial products instead of 4.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Sqr128(UInt128 a, out UInt128 lo, out UInt128 hi)
    {
        ulong a0 = (ulong)a;
        ulong a1 = (ulong)(a >> 64);

        // Three partial products
        UInt128 p00 = (UInt128)a0 * a0;
        UInt128 p01 = (UInt128)a0 * a1; // p10 == p01
        UInt128 p11 = (UInt128)a1 * a1;

        // mid = 2 * p01, detect overflow
        UInt128 mid = p01 << 1;
        ulong midCarry = (ulong)(p01 >> 127); // carry from left shift (0 or 1)
        // Note: if p01 has bit 127 set, then p01 << 1 loses that bit.
        // midCarry captures it.

        lo = p00 + (mid << 64);
        ulong loCarry = (lo < p00) ? 1u : 0u;

        hi = p11 + (mid >> 64) + ((UInt128)midCarry << 64) + loCarry;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp operator +(Fp a, Fp b)
    {
        UInt128 sum = a._value + b._value;
        // Both operands < p < 2^127, so sum < 2^128, fits in UInt128
        // But may be >= p
        if (sum >= PMask)
        {
            sum -= PMask;
            if (sum >= PMask)
                sum -= PMask;
        }
        return new Fp(sum, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp operator -(Fp a, Fp b)
    {
        if (a._value >= b._value)
            return new Fp(a._value - b._value, false);
        else
            return new Fp(PMask - b._value + a._value, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp operator -(Fp a)
    {
        if (a._value == 0)
            return Zero;
        return new Fp(PMask - a._value, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp operator *(Fp a, Fp b)
    {
        Mul128(a._value, b._value, out UInt128 lo, out UInt128 hi);
        return new Fp(Reduce256(lo, hi), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fp Square()
    {
        Sqr128(_value, out UInt128 lo, out UInt128 hi);
        return new Fp(Reduce256(lo, hi), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Fp Div2()
    {
        // Division by 2: if even, just shift; if odd, add p first
        if ((_value & 1) == 0)
            return new Fp(_value >> 1, false);
        else
            return new Fp((_value + PMask) >> 1, false);
    }

    /// <summary>
    /// Computes the multiplicative inverse using Fermat's little theorem: a^(-1) = a^(p-2) mod p
    /// </summary>
    public Fp Inverse()
    {
        if (_value == 0)
            throw new DivideByZeroException("Cannot invert zero");

        return Pow(InvExp);
    }

    /// <summary>
    /// Computes a^exp mod p using binary exponentiation with UInt128 exponent.
    /// </summary>
    public Fp Pow(UInt128 exp)
    {
        var result = One;
        var baseVal = this;

        while (exp > 0)
        {
            if ((exp & 1) == 1)
                result *= baseVal;
            baseVal = baseVal.Square();
            exp >>= 1;
        }

        return result;
    }

    /// <summary>
    /// Computes a^exp mod p using binary exponentiation with BigInteger exponent (backward compat).
    /// </summary>
    public Fp Pow(BigInteger exp)
    {
        // For exponents that fit in UInt128 (which p-2 does), convert and use fast path
        if (exp >= 0 && exp <= (BigInteger)(UInt128.MaxValue))
        {
            var bytes = exp.ToByteArray(isUnsigned: true, isBigEndian: false);
            UInt128 exp128 = 0;
            for (int i = bytes.Length - 1; i >= 0; i--)
                exp128 = (exp128 << 8) | bytes[i];
            return Pow(exp128);
        }

        // Fallback for very large exponents (shouldn't happen in practice)
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
    /// For p = 3 (mod 4), sqrt(a) = a^((p+1)/4) = a^(2^125)
    /// </summary>
    public Fp? Sqrt()
    {
        if (_value == 0)
            return Zero;

        var candidate = Pow(SqrtExp);

        // Verify
        if (candidate.Square()._value == _value)
            return candidate;

        return null;
    }

    /// <summary>
    /// Creates an Fp from two 64-bit values (little-endian).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp FromU64LE(ulong lo, ulong hi)
    {
        var hiMasked = hi & 0x7FFFFFFFFFFFFFFFUL;
        UInt128 value = ((UInt128)hiMasked << 64) | lo;
        return new Fp(Reduce(value), false);
    }

    /// <summary>
    /// Converts to 16 bytes (little-endian).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ToBytes(Span<byte> output)
    {
        if (output.Length < 16)
            throw new ArgumentException("Output must be at least 16 bytes", nameof(output));

        BinaryPrimitives.WriteUInt64LittleEndian(output, (ulong)_value);
        BinaryPrimitives.WriteUInt64LittleEndian(output.Slice(8), (ulong)(_value >> 64));
    }

    /// <summary>
    /// Creates an Fp from 16 bytes (little-endian).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Fp FromBytes(ReadOnlySpan<byte> input)
    {
        if (input.Length < 16)
            throw new ArgumentException("Input must be at least 16 bytes", nameof(input));

        ulong lo = BinaryPrimitives.ReadUInt64LittleEndian(input);
        ulong hi = BinaryPrimitives.ReadUInt64LittleEndian(input.Slice(8));
        UInt128 value = ((UInt128)hi << 64) | lo;
        return new Fp(Reduce(value), false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Fp other) => _value == other._value;
    public override bool Equals(object? obj) => obj is Fp other && Equals(other);
    public override int GetHashCode() => _value.GetHashCode();
    public static bool operator ==(Fp left, Fp right) => left._value == right._value;
    public static bool operator !=(Fp left, Fp right) => left._value != right._value;
    public override string ToString() => Value.ToString("X");

    public bool IsZero => _value == 0;
}
