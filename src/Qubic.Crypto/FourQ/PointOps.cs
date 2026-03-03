using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Optimized point operations for FourQ using multiple coordinate representations.
/// Ported from qubic-cli k12_and_key_utils.h (Microsoft FourQlib).
/// </summary>
public static class PointOps
{
    // Curve parameter d for FourQ: d = d0 + d1*i
    // d = { 0x0000000000000142, 0x00000000000000E4, 0xB3821488F1FC0C8D, 0x5E472F846657E0FC }
    private static readonly Fp2 D = new(
        Fp.FromU64LE(0x0000000000000142UL, 0x00000000000000E4UL),
        Fp.FromU64LE(0xB3821488F1FC0C8DUL, 0x5E472F846657E0FCUL)
    );

    /// <summary>
    /// Point doubling: P = 2*P.
    /// Uses 4 Fp2 squarings + 3 Fp2 multiplications + additions/subtractions.
    /// Port of eccdouble() from C++ (lines 1406-1422).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccDouble(ref PointExtProj P)
    {
        var t1 = P.X.Square();           // t1 = X1^2
        var t2 = P.Y.Square();           // t2 = Y1^2
        var xpy = P.X + P.Y;             // t3 = X1+Y1
        P.Tb = t1 + t2;                  // Tbfinal = X1^2+Y1^2
        t1 = t2 - t1;                    // t1 = Y1^2-X1^2
        P.Ta = xpy.Square();             // Ta = (X1+Y1)^2
        t2 = P.Z.Square();               // t2 = Z1^2
        P.Ta = P.Ta - P.Tb;              // Tafinal = 2X1*Y1 = (X1+Y1)^2-(X1^2+Y1^2)
        t2 = t2 + t2 - t1;              // t2 = 2Z1^2-(Y1^2-X1^2)
        P.Y = t1 * P.Tb;                 // Yfinal = (Y1^2-X1^2)(X1^2+Y1^2)
        P.X = t2 * P.Ta;                 // Xfinal = 2X1*Y1*[2Z1^2-(Y1^2-X1^2)]
        P.Z = t1 * t2;                   // Zfinal = (Y1^2-X1^2)[2Z1^2-(Y1^2-X1^2)]
    }

    /// <summary>
    /// Mixed point addition: P = P + Q where Q has Z=1 (from precomputed table).
    /// 7 Fp2 multiplications + additions/subtractions.
    /// Port of eccmadd() from C++ (lines 1475-1493).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccMadd(in PointPrecomp Q, ref PointExtProj P)
    {
        var ta = P.Ta * P.Tb;             // Ta = T1 = Ta*Tb
        var t1 = P.Z + P.Z;              // t1 = 2Z1
        ta = ta * Q.T2;                   // Ta = 2dT1*t2
        var z = P.X + P.Y;               // Z = (X1+Y1)
        var tb = P.Y - P.X;              // Tb = (Y1-X1)
        var t2 = t1 - ta;                // t2 = theta
        t1 = t1 + ta;                    // t1 = alpha
        ta = Q.XY * z;                   // Ta = (X1+Y1)(x2+y2)
        var x = Q.YX * tb;               // X = (Y1-X1)(y2-x2)
        P.Z = t1 * t2;                   // Zfinal = theta*alpha
        P.Tb = ta - x;                   // Tbfinal = beta
        P.Ta = ta + x;                   // Tafinal = omega
        P.X = P.Tb * t2;                 // Xfinal = beta*theta
        P.Y = P.Ta * t1;                 // Yfinal = alpha*omega
    }

    /// <summary>
    /// Core point addition: R = P + Q using precomputed representations.
    /// Port of eccadd_core() from C++ (lines 1424-1438).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccAddCore(in PointExtProjPrecomp P, in PointExtProjPrecomp Q, ref PointExtProj R)
    {
        R.Z = P.T2 * Q.T2;               // Z = 2dT1*T2
        var t1 = P.Z2 * Q.Z2;            // t1 = 2Z1*Z2
        R.X = P.XY * Q.XY;               // X = (X1+Y1)(X2+Y2)
        R.Y = P.YX * Q.YX;               // Y = (Y1-X1)(Y2-X2)
        var t2 = t1 - R.Z;               // t2 = theta
        t1 = t1 + R.Z;                   // t1 = alpha
        R.Tb = R.X - R.Y;                // Tbfinal = beta
        R.Ta = R.X + R.Y;                // Tafinal = omega
        R.X = R.Tb * t2;                 // Xfinal = beta*theta
        R.Z = t1 * t2;                   // Zfinal = theta*alpha
        R.Y = R.Ta * t1;                 // Yfinal = alpha*omega
    }

    /// <summary>
    /// Complete point addition: P = P + Q.
    /// Converts P to R3 form, then calls EccAddCore.
    /// Port of eccadd() from C++ (lines 1441-1447).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccAdd(in PointExtProjPrecomp Q, ref PointExtProj P)
    {
        PointExtProjPrecomp R;
        R1ToR3(in P, out R);
        EccAddCore(in Q, in R, ref P);
    }

    /// <summary>
    /// Conversion: (X,Y,Z,Ta,Tb) → (X+Y, Y-X, 2Z, 2dT) where T = Ta*Tb.
    /// Port of R1_to_R2() from C++ (lines 1381-1388).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void R1ToR2(in PointExtProj P, out PointExtProjPrecomp Q)
    {
        var t = P.Ta + P.Ta;             // T = 2*Ta
        Q.XY = P.X + P.Y;               // QX = X+Y
        Q.YX = P.Y - P.X;               // QY = Y-X
        t = t * P.Tb;                    // T = 2*Ta*Tb = 2*T
        Q.Z2 = P.Z + P.Z;               // QZ = 2*Z
        Q.T2 = t * D;                    // QT = 2d*T
    }

    /// <summary>
    /// Conversion: (X,Y,Z,Ta,Tb) → (X+Y, Y-X, Z, T) where T = Ta*Tb.
    /// Port of R1_to_R3() from C++ (lines 1391-1396).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void R1ToR3(in PointExtProj P, out PointExtProjPrecomp Q)
    {
        Q.XY = P.X + P.Y;               // XQ = (X1+Y1)
        Q.YX = P.Y - P.X;               // YQ = (Y1-X1)
        Q.T2 = P.Ta * P.Tb;             // TQ = T1
        Q.Z2 = P.Z;
    }

    /// <summary>
    /// Normalize a projective point to affine coordinates.
    /// Port of eccnorm() from C++ (lines 1357-1378).
    /// </summary>
    public static void EccNorm(in PointExtProj P, out Fp2 x, out Fp2 y)
    {
        var zInv = P.Z.Inverse();
        x = P.X * zInv;
        y = P.Y * zInv;
    }

    /// <summary>
    /// Negate a precomputed point: -(x+y, y-x, 2dt) = (y-x, x+y, -2dt).
    /// Swap XY↔YX and negate T2.
    /// Port of eccneg_precomp() from C++ (lines 1814-1819).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccNeg(in PointPrecomp P, out PointPrecomp Q)
    {
        Q.XY = P.YX;
        Q.YX = P.XY;
        Q.T2 = -P.T2;
    }

    /// <summary>
    /// Negate a precomputed extended projective point.
    /// Swap XY↔YX and negate T2.
    /// Port of eccneg_extproj_precomp() from C++ (lines 1805-1811).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccNegExtProj(in PointExtProjPrecomp P, out PointExtProjPrecomp Q)
    {
        Q.XY = P.YX;
        Q.YX = P.XY;
        Q.Z2 = P.Z2;
        Q.T2 = -P.T2;
    }

    /// <summary>
    /// Generate precomputation table for double scalar multiplication.
    /// Creates 4 entries: [P, 3P, 5P, 7P] in (X+Y,Y-X,2Z,2dT) form.
    /// Port of ecc_precomp_double() from C++ (lines 1912-1929).
    /// </summary>
    public static void EccPrecompDouble(ref PointExtProj P, PointExtProjPrecomp[] table)
    {
        R1ToR2(in P, out table[0]);         // Table[0] = P in (X+Y,Y-X,2Z,2dT)
        EccDouble(ref P);                   // A = 2*P
        PointExtProjPrecomp PP;
        R1ToR3(in P, out PP);               // PP = 2P in (X+Y,Y-X,Z,T)

        var Q = new PointExtProj();
        EccAddCore(in table[0], in PP, ref Q);  // Q = 3P
        R1ToR2(in Q, out table[1]);

        EccAddCore(in table[1], in PP, ref Q);  // Q = 5P
        R1ToR2(in Q, out table[2]);

        EccAddCore(in table[2], in PP, ref Q);  // Q = 7P
        R1ToR2(in Q, out table[3]);
    }
}
