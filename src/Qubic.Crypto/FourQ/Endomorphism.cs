using System.Runtime.CompilerServices;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// FourQ endomorphisms φ and ψ, used for scalar decomposition in double scalar multiplication.
/// Port of ecc_tau, ecc_tau_dual, ecc_delphidel, ecc_delpsidel, ecc_phi, ecc_psi
/// from qubic-cli k12_and_key_utils.h (lines 1685-1803).
/// </summary>
public static class Endomorphism
{
    // Fixed GF(p^2) constants for the endomorphisms (from C++ lines 671-686)
    private static readonly Fp2 Ctau1 = new(
        Fp.FromU64LE(0x74DCD57CEBCE74C3UL, 0x1964DE2C3AFAD20CUL),
        Fp.FromU64LE(0x12UL, 0x0CUL));

    private static readonly Fp2 CtauDual1 = new(
        Fp.FromU64LE(0x9ECAA6D9DECDF034UL, 0x4AA740EB23058652UL),
        Fp.FromU64LE(0x11UL, 0x7FFFFFFFFFFFFFF4UL));

    private static readonly Fp2 Cphi0 = new(
        Fp.FromU64LE(0xFFFFFFFFFFFFFFF7UL, 0x05UL),
        Fp.FromU64LE(0x4F65536CEF66F81AUL, 0x2553A0759182C329UL));

    private static readonly Fp2 Cphi1 = new(
        Fp.FromU64LE(0x07UL, 0x05UL),
        Fp.FromU64LE(0x334D90E9E28296F9UL, 0x62C8CAA0C50C62CFUL));

    private static readonly Fp2 Cphi2 = new(
        Fp.FromU64LE(0x15UL, 0x0FUL),
        Fp.FromU64LE(0x2C2CB7154F1DF391UL, 0x78DF262B6C9B5C98UL));

    private static readonly Fp2 Cphi3 = new(
        Fp.FromU64LE(0x03UL, 0x02UL),
        Fp.FromU64LE(0x92440457A7962EA4UL, 0x5084C6491D76342AUL));

    private static readonly Fp2 Cphi4 = new(
        Fp.FromU64LE(0x03UL, 0x03UL),
        Fp.FromU64LE(0xA1098C923AEC6855UL, 0x12440457A7962EA4UL));

    private static readonly Fp2 Cphi5 = new(
        Fp.FromU64LE(0x0FUL, 0x0AUL),
        Fp.FromU64LE(0x669B21D3C5052DF3UL, 0x459195418A18C59EUL));

    private static readonly Fp2 Cphi6 = new(
        Fp.FromU64LE(0x18UL, 0x12UL),
        Fp.FromU64LE(0xCD3643A78A0A5BE7UL, 0x0B232A8314318B3CUL));

    private static readonly Fp2 Cphi7 = new(
        Fp.FromU64LE(0x23UL, 0x18UL),
        Fp.FromU64LE(0x66C183035F48781AUL, 0x3963BC1C99E2EA1AUL));

    private static readonly Fp2 Cphi8 = new(
        Fp.FromU64LE(0xF0UL, 0xAAUL),
        Fp.FromU64LE(0x44E251582B5D0EF0UL, 0x1F529F860316CBE5UL));

    private static readonly Fp2 Cphi9 = new(
        Fp.FromU64LE(0xBEFUL, 0x870UL),
        Fp.FromU64LE(0x14D3E48976E2505UL, 0xFD52E9CFE00375BUL));

    private static readonly Fp2 Cpsi1 = new(
        Fp.FromU64LE(0xEDF07F4767E346EFUL, 0x2AF99E9A83D54A02UL),
        Fp.FromU64LE(0x13AUL, 0xDEUL));

    private static readonly Fp2 Cpsi2 = new(
        Fp.FromU64LE(0x143UL, 0xE4UL),
        Fp.FromU64LE(0x4C7DEB770E03F372UL, 0x21B8D07B99A81F03UL));

    private static readonly Fp2 Cpsi3 = new(
        Fp.FromU64LE(0x09UL, 0x06UL),
        Fp.FromU64LE(0x3A6E6ABE75E73A61UL, 0x4CB26F161D7D6906UL));

    private static readonly Fp2 Cpsi4 = new(
        Fp.FromU64LE(0xFFFFFFFFFFFFFFF6UL, 0x7FFFFFFFFFFFFFF9UL),
        Fp.FromU64LE(0xC59195418A18C59EUL, 0x334D90E9E28296F9UL));

    /// <summary>
    /// Apply tau mapping: P = tau(P).
    /// Port of ecc_tau() from C++ (lines 1685-1701).
    /// </summary>
    public static void EccTau(ref PointExtProj P)
    {
        var t0 = P.X.Square();           // t0 = X^2
        var t1 = P.Y.Square();           // t1 = Y^2
        P.X = P.X * P.Y;                 // X = X*Y
        P.Y = P.Z.Square();              // Y = Z^2
        P.Z = t0 + t1;                   // Z = X^2 + Y^2
        t0 = t1 - t0;                    // t0 = Y^2 - X^2
        P.Y = P.Y + P.Y;                 // Y = 2*Z^2
        P.X = P.X * t0;                  // X = X*Y*(Y^2-X^2)
        P.Y = P.Y - t0;                  // Y = 2*Z^2 - (Y^2-X^2)
        P.X = P.X * Ctau1;               // X = X * ctau1
        P.Y = P.Y * P.Z;                 // Y = Y * Z
        P.Z = t0 * P.Z;                  // Z = t0 * Z
    }

    /// <summary>
    /// Apply tau_dual mapping: P = tau_dual(P).
    /// Port of ecc_tau_dual() from C++ (lines 1703-1719).
    /// </summary>
    public static void EccTauDual(ref PointExtProj P)
    {
        var t0 = P.X.Square();           // t0 = X^2
        P.Ta = P.Z.Square();             // Ta = Z^2
        var t1 = P.Y.Square();           // t1 = Y^2
        P.Z = P.Ta + P.Ta;               // Z = 2*Z^2
        P.Ta = t1 - t0;                  // Ta = Y^2 - X^2 (Tafinal)
        t0 = t0 + t1;                    // t0 = X^2 + Y^2
        P.X = P.X * P.Y;                 // X = X*Y
        P.Z = P.Z - P.Ta;               // Z = 2*Z^2 - (Y^2-X^2)
        P.Tb = P.X * CtauDual1;          // Tb = ctaudual1*X*Y (Tbfinal)
        P.Y = P.Z * P.Ta;               // Y = Z * Ta (Yfinal)
        P.X = P.Tb * t0;                 // X = Tb * (X^2+Y^2) (Xfinal)
        P.Z = P.Z * t0;                  // Z = Z * (X^2+Y^2) (Zfinal)
    }

    /// <summary>
    /// Apply delta_phi_delta mapping: P = delta(phi_W(delta_inv(P))).
    /// Port of ecc_delphidel() from C++ (lines 1721-1764).
    /// </summary>
    public static void EccDelPhiDel(ref PointExtProj P)
    {
        var t4 = P.Z.Square();           // t4 = Z^2
        var t3 = P.Y * P.Z;             // t3 = Y*Z
        var t0 = t4 * Cphi4;             // t0 = cphi4*Z^2
        var t2 = P.Y.Square();           // t2 = Y^2
        t0 = t0 + t2;                    // t0 = cphi4*Z^2 + Y^2
        var t1 = t3 * Cphi3;             // t1 = cphi3*Y*Z
        var t5 = t0 - t1;               // t5 = t0 - cphi3*Y*Z
        t0 = t0 + t1;                    // t0 = t0 + cphi3*Y*Z
        t0 = t0 * P.Z;                   // t0 = t0 * Z
        t1 = t3 * Cphi1;                 // t1 = cphi1*Y*Z
        t0 = t0 * t5;                    // t0 = t0 * t5
        t5 = t4 * Cphi2;                 // t5 = cphi2*Z^2
        t5 = t2 + t5;                    // t5 = Y^2 + cphi2*Z^2
        var t6 = t1 - t5;               // t6 = cphi1*Y*Z - (Y^2 + cphi2*Z^2)
        t1 = t1 + t5;                    // t1 = cphi1*Y*Z + (Y^2 + cphi2*Z^2)
        t6 = t1 * t6;                    // t6 = t1 * t6
        t6 = t6 * Cphi0;                 // t6 = cphi0 * t6
        P.X = P.X * t6;                  // X = X * t6
        t6 = t2.Square();                // t6 = Y^4
        t2 = t3.Square();                // t2 = (Y*Z)^2
        t3 = t4.Square();                // t3 = Z^4
        t1 = t2 * Cphi8;                 // t1 = cphi8*(Y*Z)^2
        t5 = t3 * Cphi9;                 // t5 = cphi9*Z^4
        t1 = t1 + t6;                    // t1 = cphi8*(Y*Z)^2 + Y^4
        t2 = t2 * Cphi6;                 // t2 = cphi6*(Y*Z)^2
        t3 = t3 * Cphi7;                 // t3 = cphi7*Z^4
        t1 = t1 + t5;                    // t1 = cphi8*(Y*Z)^2 + Y^4 + cphi9*Z^4
        t2 = t2 + t3;                    // t2 = cphi6*(Y*Z)^2 + cphi7*Z^4
        t1 = P.Y * t1;                   // t1 = Y * t1  (P.Y still original)
        P.Y = t6 + t2;                   // Y = Y^4 + cphi6*(Y*Z)^2 + cphi7*Z^4
        P.X = P.X * t1;                  // X = X * t1
        P.Y = P.Y * Cphi5;              // Y = cphi5 * Y
        P.X = P.X.Conjugate();          // X = X^p (negate imaginary part)
        P.Y = P.Y * P.Z;                // Y = Y * Z  (P.Z still original)
        P.Z = t0 * t1;                  // Z = t0 * t1
        P.Y = P.Y * t0;                 // Y = Y * t0
        P.Z = P.Z.Conjugate();          // Z = Z^p
        P.Y = P.Y.Conjugate();          // Y = Y^p
    }

    /// <summary>
    /// Apply delta_psi_delta mapping: P = delta(psi_W(delta_inv(P))).
    /// Port of ecc_delpsidel() from C++ (lines 1766-1789).
    /// </summary>
    public static void EccDelPsiDel(ref PointExtProj P)
    {
        P.X = P.X.Conjugate();           // X = X^p
        P.Z = P.Z.Conjugate();           // Z = Z^p
        P.Y = P.Y.Conjugate();           // Y = Y^p
        var t2 = P.Z.Square();           // t2 = Z^p^2
        var t0 = P.X.Square();           // t0 = X^p^2
        P.X = P.X * t2;                  // X = X^p * Z^p^2
        P.Z = t2 * Cpsi2;               // Z = cpsi2*Z^p^2
        var t1 = t2 * Cpsi3;             // t1 = cpsi3*Z^p^2
        t2 = t2 * Cpsi4;                 // t2 = cpsi4*Z^p^2
        P.Z = t0 + P.Z;                  // Z = X^p^2 + cpsi2*Z^p^2
        t2 = t0 + t2;                    // t2 = X^p^2 + cpsi4*Z^p^2
        t1 = t0 + t1;                    // t1 = X^p^2 + cpsi3*Z^p^2
        t2 = -t2;                        // t2 = -(X^p^2 + cpsi4*Z^p^2)
        P.Z = P.Z * P.Y;                // Z = Y^p * (X^p^2 + cpsi2*Z^p^2)
        P.X = P.X * t2;                  // X = -X^p*Z^p^2*(X^p^2 + cpsi4*Z^p^2)
        P.Y = t1 * P.Z;                 // Y = t1 * Z (Yfinal)
        P.X = P.X * Cpsi1;              // X = cpsi1 * X (Xfinal)
        P.Z = P.Z * t2;                 // Z = Z * t2 (Zfinal)
    }

    /// <summary>
    /// Apply phi mapping: P = phi(P) = tau_dual(delphidel(tau(P))).
    /// Port of ecc_phi() from C++ (lines 1798-1803).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccPhi(ref PointExtProj P)
    {
        EccTau(ref P);
        EccDelPhiDel(ref P);
        EccTauDual(ref P);
    }

    /// <summary>
    /// Apply psi mapping: P = psi(P) = tau_dual(delpsidel(tau(P))).
    /// Port of ecc_psi() from C++ (lines 1791-1796).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EccPsi(ref PointExtProj P)
    {
        EccTau(ref P);
        EccDelPsiDel(ref P);
        EccTauDual(ref P);
    }
}
