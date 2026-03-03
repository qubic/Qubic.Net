using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Scalar decomposition and wNAF recoding for double scalar multiplication.
/// Port of decompose() and wNAF_recode() from qubic-cli k12_and_key_utils.h (lines 1822-1910).
/// </summary>
public static class ScalarDecompose
{
    // Decomposition matrix B (from C++ lines 602-617)
    private const ulong B11 = 0xF6F900D81F5F5E6A;
    private const ulong B12 = 0x1363E862C22A2DA0;
    private const ulong B13 = 0xF8BD9FCE1337FCF1;
    private const ulong B14 = 0x084F739986B9E651;
    private const ulong B21 = 0xE2B6A4157B033D2C;
    private const ulong B22 = 0x0000000000000001;
    private const ulong B23 = 0xFFFFFFFFFFFFFFFF;
    private const ulong B24 = 0xDA243A43722E9830;
    private const ulong B31 = 0xE85452E2DCE0FCFE;
    private const ulong B32 = 0xFD3BDEE51C7725AF;
    private const ulong B33 = 0x2E4D21C98927C49F;
    private const ulong B34 = 0xF56190BB3FD13269;
    private const ulong B41 = 0xEC91CBF56EF737C1;
    private const ulong B42 = 0xCEDD20D23C1F00CE;
    private const ulong B43 = 0x068A49F02AA8A9B5;
    private const ulong B44 = 0x18D5087896DE0AEA;

    // Offset constants C (from C++ lines 618-621)
    private const ulong C1 = 0x72482C5251A4559C;
    private const ulong C2 = 0x59F95B0ADD276F6C;
    private const ulong C3 = 0x7DD2D17C4625FA78;
    private const ulong C4 = 0x6BC57DEF56CE8877;

    // Precomputed integers for fast-Babai rounding (from C++ lines 689-692)
    private static readonly ulong[] Ell1 = { 0x259686E09D1A7D4F, 0xF75682ACE6A6BD66, 0xFC5BB5C5EA2BE5DF, 0x07 };
    private static readonly ulong[] Ell2 = { 0xD1BA1D84DD627AFB, 0x2BD235580F468D8D, 0x8FD4B04CAA6C0F8A, 0x03 };
    private static readonly ulong[] Ell3 = { 0x9B291A33678C203C, 0xC42BD6C965DCA902, 0xD038BF8D0BFFBAF6, 0x00 };
    private static readonly ulong[] Ell4 = { 0x12E5666B77E7FDC0, 0x81CBDC3714983D82, 0x1B073877A22D8410, 0x03 };

    /// <summary>
    /// Truncated multiply: returns floor(s * C / 2^256) as a single ulong.
    /// Direct port of mul_truncate() from C++ (lines 1822-1846) using _umul128/_addcarry_u64 pattern.
    /// </summary>
    private static ulong MulTruncate(ReadOnlySpan<ulong> s, ulong[] C)
    {
        // Each 64x64 multiply produces a 128-bit result (hi:lo)
        UInt128 p;
        ulong high00, low10, high10, low01, high01, low20, high20, low02, high02;
        ulong low11, high11, low03, high03, low30, high30, low12, high12, high21;

        p = (UInt128)s[0] * C[0]; high00 = (ulong)(p >> 64);
        p = (UInt128)s[1] * C[0]; low10 = (ulong)p; high10 = (ulong)(p >> 64);

        // _addcarry_u64(_addcarry_u64(0, high00, low10, &t0), high10, 0, &t1)
        UInt128 acc = (UInt128)high00 + low10;
        ulong t0 = (ulong)acc;
        ulong t1 = high10 + (ulong)(acc >> 64);

        p = (UInt128)s[0] * C[1]; low01 = (ulong)p; high01 = (ulong)(p >> 64);

        // t2 = _addcarry_u64(_addcarry_u64(0, t0, low01, &t0), t1, high01, &t3)
        acc = (UInt128)t0 + low01;
        t0 = (ulong)acc;
        acc = (UInt128)t1 + high01 + (ulong)(acc >> 64);
        ulong t3 = (ulong)acc;
        ulong t2 = (ulong)(acc >> 64);

        p = (UInt128)s[2] * C[0]; low20 = (ulong)p; high20 = (ulong)(p >> 64);

        // _addcarry_u64(_addcarry_u64(0, t3, low20, &t4), t2, high20, &t5)
        acc = (UInt128)t3 + low20;
        ulong t4 = (ulong)acc;
        acc = (UInt128)t2 + high20 + (ulong)(acc >> 64);
        ulong t5 = (ulong)acc;

        p = (UInt128)s[0] * C[2]; low02 = (ulong)p; high02 = (ulong)(p >> 64);

        // t6 = _addcarry_u64(_addcarry_u64(0, t4, low02, &t7), t5, high02, &t8)
        acc = (UInt128)t4 + low02;
        ulong t7 = (ulong)acc;
        acc = (UInt128)t5 + high02 + (ulong)(acc >> 64);
        ulong t8 = (ulong)acc;
        ulong t6 = (ulong)(acc >> 64);

        p = (UInt128)s[1] * C[1]; low11 = (ulong)p; high11 = (ulong)(p >> 64);

        // t9 = _addcarry_u64(_addcarry_u64(0, t7, low11, &t0), t8, high11, &t10)
        acc = (UInt128)t7 + low11;
        t0 = (ulong)acc;
        acc = (UInt128)t8 + high11 + (ulong)(acc >> 64);
        ulong t10 = (ulong)acc;
        ulong t9 = (ulong)(acc >> 64);

        p = (UInt128)s[0] * C[3]; low03 = (ulong)p; high03 = (ulong)(p >> 64);

        // _addcarry_u64(_addcarry_u64(0, t10, low03, &t11), t6 + t9, high03, &t12)
        acc = (UInt128)t10 + low03;
        ulong t11 = (ulong)acc;
        acc = (UInt128)(t6 + t9) + high03 + (ulong)(acc >> 64);
        ulong t12 = (ulong)acc;

        p = (UInt128)s[3] * C[0]; low30 = (ulong)p; high30 = (ulong)(p >> 64);

        // _addcarry_u64(_addcarry_u64(0, t11, low30, &t13), t12, high30, &t14)
        acc = (UInt128)t11 + low30;
        ulong t13 = (ulong)acc;
        acc = (UInt128)t12 + high30 + (ulong)(acc >> 64);
        ulong t14 = (ulong)acc;

        p = (UInt128)s[1] * C[2]; low12 = (ulong)p; high12 = (ulong)(p >> 64);

        // _addcarry_u64(_addcarry_u64(0, t13, low12, &t15), t14, high12, &t16)
        acc = (UInt128)t13 + low12;
        ulong t15 = (ulong)acc;
        acc = (UInt128)t14 + high12 + (ulong)(acc >> 64);
        ulong t16 = (ulong)acc;

        // return _addcarry_u64(0, t15, _umul128(s[2], C[1], &high21), &t0) + t16 + high21 + s[1]*C[3] + s[2]*C[2] + s[3]*C[1]
        p = (UInt128)s[2] * C[1]; ulong low21 = (ulong)p; high21 = (ulong)(p >> 64);
        acc = (UInt128)t15 + low21;
        ulong carry = (ulong)(acc >> 64);

        return carry + t16 + high21 + s[1] * C[3] + s[2] * C[2] + s[3] * C[1];
    }

    /// <summary>
    /// Scalar decomposition: decomposes a 256-bit scalar into four ~64-bit sub-scalars.
    /// Port of decompose() from C++ (lines 1848-1865).
    /// </summary>
    public static void Decompose(ReadOnlySpan<ulong> k, Span<ulong> scalars)
    {
        ulong a1 = MulTruncate(k, Ell1);
        ulong a2 = MulTruncate(k, Ell2);
        ulong a3 = MulTruncate(k, Ell3);
        ulong a4 = MulTruncate(k, Ell4);

        scalars[0] = a1 * B11 + a2 * B21 + a3 * B31 + a4 * B41 + C1 + k[0];
        scalars[1] = a1 * B12 + a2 * B22 + a3 * B32 + a4 * B42 + C2;
        scalars[2] = a1 * B13 + a2 * B23 + a3 * B33 + a4 * B43 + C3;
        scalars[3] = a1 * B14 + a2 * B24 + a3 * B34 + a4 * B44 + C4;

        // Make scalars[0] odd
        if ((scalars[0] & 1) == 0)
        {
            scalars[0] -= B41;
            scalars[1] -= B42;
            scalars[2] -= B43;
            scalars[3] -= B44;
        }
    }

    /// <summary>
    /// Computes wNAF recoding of a scalar.
    /// Digits are in set {0, ±1, ±3, ..., ±(2^(w-1)-1)}.
    /// Port of wNAF_recode() from C++ (lines 1867-1910).
    /// </summary>
    /// <param name="scalar">The scalar to recode (max 64 bits)</param>
    /// <param name="w">Window width (4 or 8)</param>
    /// <param name="digits">Output array of 65 signed digits</param>
    public static void WNafRecode(ulong scalar, int w, Span<sbyte> digits)
    {
        int val1 = (1 << (w - 1)) - 1;  // 2^(w-1) - 1
        int val2 = 1 << w;               // 2^w
        ulong mask = (ulong)val2 - 1;    // 2^w - 1
        int index = 0;

        while (scalar != 0)
        {
            int digit = (int)(scalar & 1);
            if (digit == 0)
            {
                scalar >>= 1;
                digits[index] = 0;
            }
            else
            {
                digit = (int)(scalar & mask);
                scalar >>= w;

                if (digit > val1)
                {
                    digit -= val2;
                }
                if (digit < 0)
                {
                    scalar++;
                }
                digits[index] = (sbyte)digit;

                if (scalar != 0)
                {
                    for (int i = 0; i < w - 1; i++)
                    {
                        digits[++index] = 0;
                    }
                }
            }
            index++;
        }

        // Zero remaining digits
        for (int i = index; i < 65; i++)
        {
            digits[i] = 0;
        }
    }

    /// <summary>
    /// Reads a 256-bit scalar from bytes into 4 ulongs (little-endian).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ReadScalar(ReadOnlySpan<byte> bytes, Span<ulong> scalar)
    {
        scalar[0] = BinaryPrimitives.ReadUInt64LittleEndian(bytes);
        scalar[1] = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(8));
        scalar[2] = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(16));
        scalar[3] = BinaryPrimitives.ReadUInt64LittleEndian(bytes.Slice(24));
    }
}
