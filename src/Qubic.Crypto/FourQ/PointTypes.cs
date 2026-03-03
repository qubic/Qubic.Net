using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Extended projective point with split T (T = Ta * Tb).
/// Mutable struct for in-place operations in the hot path.
/// Coordinates: (X : Y : Z : Ta : Tb) where x = X/Z, y = Y/Z, T = XY/Z = Ta*Tb
/// </summary>
public struct PointExtProj
{
    public Fp2 X, Y, Z, Ta, Tb;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointExtProj Identity()
    {
        var p = new PointExtProj();
        p.X = Fp2.Zero;
        p.Y = Fp2.One;
        p.Z = Fp2.One;
        p.Ta = Fp2.Zero;
        p.Tb = Fp2.Zero;
        return p;
    }

    /// <summary>
    /// Sets up a point from affine coordinates (X, Y, 1, X, Y).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PointExtProj FromAffine(Fp2 x, Fp2 y)
    {
        var p = new PointExtProj();
        p.X = x;
        p.Y = y;
        p.Z = Fp2.One;
        p.Ta = x;
        p.Tb = y;
        return p;
    }
}

/// <summary>
/// Precomputed point in extended projective coordinates: (X+Y, Y-X, 2Z, 2dT).
/// Used for general point addition (eccadd).
/// </summary>
public struct PointExtProjPrecomp
{
    public Fp2 XY; // X + Y
    public Fp2 YX; // Y - X
    public Fp2 Z2; // 2 * Z
    public Fp2 T2; // 2 * d * T
}

/// <summary>
/// Precomputed affine point: (x+y, y-x, 2*d*t) with implicit Z = 1.
/// Used for mixed addition from precomputed tables (eccmadd).
/// </summary>
public struct PointPrecomp
{
    public Fp2 XY; // x + y
    public Fp2 YX; // y - x
    public Fp2 T2; // 2 * d * t
}
