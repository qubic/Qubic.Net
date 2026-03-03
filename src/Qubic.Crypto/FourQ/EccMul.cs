using System;
using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Optimized elliptic curve scalar multiplication for FourQ.
/// Uses precomputed tables and comb method for fixed-base multiplication.
/// Port of ecc_mul_fixed() from qubic-cli k12_and_key_utils.h.
/// </summary>
public static class EccMul
{
    // Curve order constants (little-endian)
    private const ulong CURVE_ORDER_0 = 0x2FB2540EC7768CE7;
    private const ulong CURVE_ORDER_1 = 0xDFBD004DFE0F7999;
    private const ulong CURVE_ORDER_2 = 0xF05397829CBC14E5;
    private const ulong CURVE_ORDER_3 = 0x0029CBC14E5E0A72;

    /// <summary>
    /// Fixed-base scalar multiplication: Q = k * G where G is the generator.
    /// Uses the precomputed FIXED_BASE_TABLE with a comb method.
    /// Returns the result as an encoded 32-byte public key.
    /// Port of ecc_mul_fixed() from C++ (lines 1495-1683).
    /// </summary>
    /// <param name="scalarBytes">32-byte scalar in little-endian (already reduced mod N)</param>
    /// <returns>Encoded 32-byte point</returns>
    public static byte[] ScalarMulFixed(ReadOnlySpan<byte> scalarBytes)
    {
        var result = new byte[32];
        ScalarMulFixed(scalarBytes, result);
        return result;
    }

    /// <summary>
    /// Fixed-base scalar multiplication into a provided output buffer.
    /// </summary>
    public static void ScalarMulFixed(ReadOnlySpan<byte> scalarBytes, Span<byte> output)
    {
        // Read scalar as 4 ulong values (little-endian)
        Span<ulong> scalar = stackalloc ulong[4];
        scalar[0] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes);
        scalar[1] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(8));
        scalar[2] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(16));
        scalar[3] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(24));

        // Make scalar odd: if even, add curve order (which is odd)
        if ((scalar[0] & 1) == 0)
        {
            ulong carry = 0;
            AddU64WithCarry(scalar[0], CURVE_ORDER_0, ref carry, out scalar[0]);
            AddU64WithCarry(scalar[1], CURVE_ORDER_1, ref carry, out scalar[1]);
            AddU64WithCarry(scalar[2], CURVE_ORDER_2, ref carry, out scalar[2]);
            AddU64WithCarry(scalar[3], CURVE_ORDER_3, ref carry, out scalar[3]);
        }

        // Shift scalar right by 1
        scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
        scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
        scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
        scalar[3] >>= 1;

        // Recode into 250 digits
        Span<uint> digits = stackalloc uint[250];

        // First 50 digits: sign row (0=positive, 0xFFFFFFFF=negative)
        for (int i = 0; i < 49; i++)
        {
            digits[i] = (uint)((scalar[0] & 1) - 1);  // 0 if bit=1, 0xFFFFFFFF if bit=0

            // Shift scalar right by 1
            scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
            scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
            scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
            scalar[3] >>= 1;
        }
        digits[49] = 0;

        // Next 200 digits: data rows
        for (int i = 50; i < 250; i++)
        {
            digits[i] = (uint)(scalar[0] & 1);

            // Shift scalar right by 1
            scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
            scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
            scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
            scalar[3] >>= 1;

            // Carry adjustment
            ulong temp = (0UL - digits[i - (i / 50) * 50]) & digits[i];

            // floor(scalar/2) + temp
            scalar[0] += temp;
            ulong c = (scalar[0] < temp) ? 1UL : 0UL;
            scalar[1] += c;
            c = (scalar[1] < c) ? 1UL : 0UL;
            scalar[2] += c;
            scalar[3] += (scalar[2] < c) ? 1UL : 0UL;
        }

        // Main computation using the precomputed table
        var R = new PointExtProj();

        // Initial point from table
        var S = Tables.LookupFixedBase(
            (int)(64 + (((((digits[249] << 1) + digits[199]) << 1) + digits[149]) << 1) + digits[99]),
            false);

        // Convert from (x+y, y-x, 2dt) to (X, Y, Z=1, Ta=X, Tb=Y)
        var xy = S.XY;
        var yx = S.YX;
        R.X = (xy - yx).Div2();          // x = ((x+y) - (y-x)) / 2
        R.Y = (xy + yx).Div2();          // y = ((x+y) + (y-x)) / 2
        R.Z = Fp2.One;
        R.Ta = R.X;
        R.Tb = R.Y;

        S = Tables.LookupFixedBase(
            (int)(48 + (((((digits[239] << 1) + digits[189]) << 1) + digits[139]) << 1) + digits[89]),
            digits[39] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(32 + (((((digits[229] << 1) + digits[179]) << 1) + digits[129]) << 1) + digits[79]),
            digits[29] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(16 + (((((digits[219] << 1) + digits[169]) << 1) + digits[119]) << 1) + digits[69]),
            digits[19] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(0 + (((((digits[209] << 1) + digits[159]) << 1) + digits[109]) << 1) + digits[59]),
            digits[9] != 0);
        PointOps.EccMadd(in S, ref R);

        // 9 iterations of: double + 5 mixed additions
        for (int iteration = 1; iteration <= 9; iteration++)
        {
            int baseDigit = 249 - iteration;

            PointOps.EccDouble(ref R);

            S = Tables.LookupFixedBase(
                (int)(64 + (((((digits[baseDigit] << 1) + digits[baseDigit - 50]) << 1) + digits[baseDigit - 100]) << 1) + digits[baseDigit - 150]),
                digits[baseDigit - 200] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(48 + (((((digits[baseDigit - 10] << 1) + digits[baseDigit - 60]) << 1) + digits[baseDigit - 110]) << 1) + digits[baseDigit - 160]),
                digits[baseDigit - 200 - 10] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(32 + (((((digits[baseDigit - 20] << 1) + digits[baseDigit - 70]) << 1) + digits[baseDigit - 120]) << 1) + digits[baseDigit - 170]),
                digits[baseDigit - 200 - 20] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(16 + (((((digits[baseDigit - 30] << 1) + digits[baseDigit - 80]) << 1) + digits[baseDigit - 130]) << 1) + digits[baseDigit - 180]),
                digits[baseDigit - 200 - 30] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(0 + (((((digits[baseDigit - 40] << 1) + digits[baseDigit - 90]) << 1) + digits[baseDigit - 140]) << 1) + digits[baseDigit - 190]),
                digits[baseDigit - 200 - 40] != 0);
            PointOps.EccMadd(in S, ref R);
        }

        // Normalize to affine and encode
        PointOps.EccNorm(in R, out var xAffine, out var yAffine);

        // Encode: y-coordinate with sign bit of x
        yAffine.ToBytes(output);
        var xSign = FourQPoint.GetXSign(xAffine);
        output[31] = (byte)((output[31] & 0x7F) | (xSign << 7));
    }

    /// <summary>
    /// Fixed-base scalar multiplication returning PointExt (for internal use by SchnorrQ).
    /// </summary>
    public static PointExt ScalarMulFixedPoint(ReadOnlySpan<byte> scalarBytes)
    {
        // Read scalar as 4 ulong values (little-endian)
        Span<ulong> scalar = stackalloc ulong[4];
        scalar[0] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes);
        scalar[1] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(8));
        scalar[2] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(16));
        scalar[3] = BinaryPrimitives.ReadUInt64LittleEndian(scalarBytes.Slice(24));

        // Make scalar odd
        if ((scalar[0] & 1) == 0)
        {
            ulong carry = 0;
            AddU64WithCarry(scalar[0], CURVE_ORDER_0, ref carry, out scalar[0]);
            AddU64WithCarry(scalar[1], CURVE_ORDER_1, ref carry, out scalar[1]);
            AddU64WithCarry(scalar[2], CURVE_ORDER_2, ref carry, out scalar[2]);
            AddU64WithCarry(scalar[3], CURVE_ORDER_3, ref carry, out scalar[3]);
        }

        // Shift right by 1
        scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
        scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
        scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
        scalar[3] >>= 1;

        // Recode
        Span<uint> digits = stackalloc uint[250];
        for (int i = 0; i < 49; i++)
        {
            digits[i] = (uint)((scalar[0] & 1) - 1);
            scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
            scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
            scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
            scalar[3] >>= 1;
        }
        digits[49] = 0;

        for (int i = 50; i < 250; i++)
        {
            digits[i] = (uint)(scalar[0] & 1);
            scalar[0] = (scalar[0] >> 1) | (scalar[1] << 63);
            scalar[1] = (scalar[1] >> 1) | (scalar[2] << 63);
            scalar[2] = (scalar[2] >> 1) | (scalar[3] << 63);
            scalar[3] >>= 1;

            ulong temp = (0UL - digits[i - (i / 50) * 50]) & digits[i];
            scalar[0] += temp;
            ulong c = (scalar[0] < temp) ? 1UL : 0UL;
            scalar[1] += c;
            c = (scalar[1] < c) ? 1UL : 0UL;
            scalar[2] += c;
            scalar[3] += (scalar[2] < c) ? 1UL : 0UL;
        }

        var R = new PointExtProj();

        var S = Tables.LookupFixedBase(
            (int)(64 + (((((digits[249] << 1) + digits[199]) << 1) + digits[149]) << 1) + digits[99]),
            false);

        var xy = S.XY;
        var yx = S.YX;
        R.X = (xy - yx).Div2();
        R.Y = (xy + yx).Div2();
        R.Z = Fp2.One;
        R.Ta = R.X;
        R.Tb = R.Y;

        S = Tables.LookupFixedBase(
            (int)(48 + (((((digits[239] << 1) + digits[189]) << 1) + digits[139]) << 1) + digits[89]),
            digits[39] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(32 + (((((digits[229] << 1) + digits[179]) << 1) + digits[129]) << 1) + digits[79]),
            digits[29] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(16 + (((((digits[219] << 1) + digits[169]) << 1) + digits[119]) << 1) + digits[69]),
            digits[19] != 0);
        PointOps.EccMadd(in S, ref R);

        S = Tables.LookupFixedBase(
            (int)(0 + (((((digits[209] << 1) + digits[159]) << 1) + digits[109]) << 1) + digits[59]),
            digits[9] != 0);
        PointOps.EccMadd(in S, ref R);

        for (int iteration = 1; iteration <= 9; iteration++)
        {
            int baseDigit = 249 - iteration;

            PointOps.EccDouble(ref R);

            S = Tables.LookupFixedBase(
                (int)(64 + (((((digits[baseDigit] << 1) + digits[baseDigit - 50]) << 1) + digits[baseDigit - 100]) << 1) + digits[baseDigit - 150]),
                digits[baseDigit - 200] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(48 + (((((digits[baseDigit - 10] << 1) + digits[baseDigit - 60]) << 1) + digits[baseDigit - 110]) << 1) + digits[baseDigit - 160]),
                digits[baseDigit - 200 - 10] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(32 + (((((digits[baseDigit - 20] << 1) + digits[baseDigit - 70]) << 1) + digits[baseDigit - 120]) << 1) + digits[baseDigit - 170]),
                digits[baseDigit - 200 - 20] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(16 + (((((digits[baseDigit - 30] << 1) + digits[baseDigit - 80]) << 1) + digits[baseDigit - 130]) << 1) + digits[baseDigit - 180]),
                digits[baseDigit - 200 - 30] != 0);
            PointOps.EccMadd(in S, ref R);

            S = Tables.LookupFixedBase(
                (int)(0 + (((((digits[baseDigit - 40] << 1) + digits[baseDigit - 90]) << 1) + digits[baseDigit - 140]) << 1) + digits[baseDigit - 190]),
                digits[baseDigit - 200 - 40] != 0);
            PointOps.EccMadd(in S, ref R);
        }

        // Return as PointExt
        PointOps.EccNorm(in R, out var xAffine, out var yAffine);
        return PointExt.FromAffine(xAffine, yAffine);
    }

    /// <summary>
    /// Double scalar multiplication: R = k*G + l*Q.
    /// Uses 4D GLV/GLS decomposition with endomorphisms phi and psi.
    /// Port of ecc_mul_double() from C++ (lines 1931-2076).
    /// Returns result as a PointExt (affine).
    /// </summary>
    /// <param name="kBytes">32-byte scalar k (for generator G)</param>
    /// <param name="lBytes">32-byte scalar l (for variable point Q)</param>
    /// <param name="qx">x-coordinate of Q</param>
    /// <param name="qy">y-coordinate of Q</param>
    /// <returns>The resulting affine point, or null if Q is invalid</returns>
    public static PointExt? ScalarMulDouble(ReadOnlySpan<byte> kBytes, ReadOnlySpan<byte> lBytes, Fp2 qx, Fp2 qy)
    {
        // Set up Q1 from affine
        var Q1 = PointExtProj.FromAffine(qx, qy);

        // Compute endomorphisms: Q2 = phi(Q1), Q3 = psi(Q1), Q4 = psi(phi(Q1))
        var Q2 = Q1;
        Endomorphism.EccPhi(ref Q2);
        var Q3 = Q1;
        Endomorphism.EccPsi(ref Q3);
        var Q4 = Q2;
        Endomorphism.EccPsi(ref Q4);

        // Decompose scalars k and l
        Span<ulong> kScalar = stackalloc ulong[4];
        Span<ulong> lScalar = stackalloc ulong[4];
        ScalarDecompose.ReadScalar(kBytes, kScalar);
        ScalarDecompose.ReadScalar(lBytes, lScalar);

        Span<ulong> kDecomp = stackalloc ulong[4];
        Span<ulong> lDecomp = stackalloc ulong[4];
        ScalarDecompose.Decompose(kScalar, kDecomp);
        ScalarDecompose.Decompose(lScalar, lDecomp);

        // wNAF recode (k: window 8, l: window 4)
        Span<sbyte> digitsK1 = stackalloc sbyte[65];
        Span<sbyte> digitsK2 = stackalloc sbyte[65];
        Span<sbyte> digitsK3 = stackalloc sbyte[65];
        Span<sbyte> digitsK4 = stackalloc sbyte[65];
        Span<sbyte> digitsL1 = stackalloc sbyte[65];
        Span<sbyte> digitsL2 = stackalloc sbyte[65];
        Span<sbyte> digitsL3 = stackalloc sbyte[65];
        Span<sbyte> digitsL4 = stackalloc sbyte[65];

        ScalarDecompose.WNafRecode(kDecomp[0], 8, digitsK1);
        ScalarDecompose.WNafRecode(kDecomp[1], 8, digitsK2);
        ScalarDecompose.WNafRecode(kDecomp[2], 8, digitsK3);
        ScalarDecompose.WNafRecode(kDecomp[3], 8, digitsK4);
        ScalarDecompose.WNafRecode(lDecomp[0], 4, digitsL1);
        ScalarDecompose.WNafRecode(lDecomp[1], 4, digitsL2);
        ScalarDecompose.WNafRecode(lDecomp[2], 4, digitsL3);
        ScalarDecompose.WNafRecode(lDecomp[3], 4, digitsL4);

        // Build precomputation tables for Q1-Q4 (4 entries each: P, 3P, 5P, 7P)
        var qTable1 = new PointExtProjPrecomp[4];
        var qTable2 = new PointExtProjPrecomp[4];
        var qTable3 = new PointExtProjPrecomp[4];
        var qTable4 = new PointExtProjPrecomp[4];
        PointOps.EccPrecompDouble(ref Q1, qTable1);
        PointOps.EccPrecompDouble(ref Q2, qTable2);
        PointOps.EccPrecompDouble(ref Q3, qTable3);
        PointOps.EccPrecompDouble(ref Q4, qTable4);

        // Initialize T as neutral point (0 : 1 : 1)
        var T = PointExtProj.Identity();

        // Main loop: 65 iterations (from i=64 down to 0)
        for (int i = 64; i >= 0; i--)
        {
            PointOps.EccDouble(ref T);

            // l-scalar additions (variable base, using Q tables)
            if (digitsL1[i] < 0)
            {
                PointOps.EccNegExtProj(in qTable1[(-digitsL1[i]) >> 1], out var U);
                PointOps.EccAdd(in U, ref T);
            }
            else if (digitsL1[i] > 0)
            {
                PointOps.EccAdd(in qTable1[digitsL1[i] >> 1], ref T);
            }

            if (digitsL2[i] < 0)
            {
                PointOps.EccNegExtProj(in qTable2[(-digitsL2[i]) >> 1], out var U);
                PointOps.EccAdd(in U, ref T);
            }
            else if (digitsL2[i] > 0)
            {
                PointOps.EccAdd(in qTable2[digitsL2[i] >> 1], ref T);
            }

            if (digitsL3[i] < 0)
            {
                PointOps.EccNegExtProj(in qTable3[(-digitsL3[i]) >> 1], out var U);
                PointOps.EccAdd(in U, ref T);
            }
            else if (digitsL3[i] > 0)
            {
                PointOps.EccAdd(in qTable3[digitsL3[i] >> 1], ref T);
            }

            if (digitsL4[i] < 0)
            {
                PointOps.EccNegExtProj(in qTable4[(-digitsL4[i]) >> 1], out var U);
                PointOps.EccAdd(in U, ref T);
            }
            else if (digitsL4[i] > 0)
            {
                PointOps.EccAdd(in qTable4[digitsL4[i] >> 1], ref T);
            }

            // k-scalar additions (fixed base, using DOUBLE_SCALAR_TABLE)
            // Table layout: 4 sub-tables of 64 points each (indices 0-63, 64-127, 128-191, 192-255)
            if (digitsK1[i] != 0)
            {
                int idx = Math.Abs(digitsK1[i]) >> 1;
                var V = Tables.LookupDoubleScalar(idx, digitsK1[i] < 0);
                PointOps.EccMadd(in V, ref T);
            }

            if (digitsK2[i] != 0)
            {
                int idx = 64 + (Math.Abs(digitsK2[i]) >> 1);
                var V = Tables.LookupDoubleScalar(idx, digitsK2[i] < 0);
                PointOps.EccMadd(in V, ref T);
            }

            if (digitsK3[i] != 0)
            {
                int idx = 128 + (Math.Abs(digitsK3[i]) >> 1);
                var V = Tables.LookupDoubleScalar(idx, digitsK3[i] < 0);
                PointOps.EccMadd(in V, ref T);
            }

            if (digitsK4[i] != 0)
            {
                int idx = 192 + (Math.Abs(digitsK4[i]) >> 1);
                var V = Tables.LookupDoubleScalar(idx, digitsK4[i] < 0);
                PointOps.EccMadd(in V, ref T);
            }
        }

        // Normalize and return
        PointOps.EccNorm(in T, out var xAffine, out var yAffine);
        return PointExt.FromAffine(xAffine, yAffine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddU64WithCarry(ulong a, ulong b, ref ulong carry, out ulong result)
    {
        ulong sum = a + b + carry;
        carry = ((a & b) | ((a | b) & ~sum)) >> 63;
        result = sum;
    }
}
