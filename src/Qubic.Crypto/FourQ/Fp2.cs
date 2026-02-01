using System;
using System.Numerics;

namespace Qubic.Crypto.FourQ;

/// <summary>
/// Quadratic extension field Fp2 = Fp[i] where i² = -1.
/// Elements are represented as a + bi where a, b ∈ Fp.
/// </summary>
public readonly struct Fp2 : IEquatable<Fp2>
{
    /// <summary>
    /// Real part
    /// </summary>
    public readonly Fp A;

    /// <summary>
    /// Imaginary part (coefficient of i)
    /// </summary>
    public readonly Fp B;

    public static readonly Fp2 Zero = new(Fp.Zero, Fp.Zero);
    public static readonly Fp2 One = new(Fp.One, Fp.Zero);
    public static readonly Fp2 I = new(Fp.Zero, Fp.One);

    public Fp2(Fp a, Fp b)
    {
        A = a;
        B = b;
    }

    public Fp2(BigInteger a, BigInteger b)
    {
        A = new Fp(a);
        B = new Fp(b);
    }

    public static Fp2 operator +(Fp2 x, Fp2 y)
    {
        return new Fp2(x.A + y.A, x.B + y.B);
    }

    public static Fp2 operator -(Fp2 x, Fp2 y)
    {
        return new Fp2(x.A - y.A, x.B - y.B);
    }

    public static Fp2 operator -(Fp2 x)
    {
        return new Fp2(-x.A, -x.B);
    }

    /// <summary>
    /// Multiplication: (a + bi)(c + di) = (ac - bd) + (ad + bc)i
    /// </summary>
    public static Fp2 operator *(Fp2 x, Fp2 y)
    {
        // Karatsuba-style: (ac - bd), (a+b)(c+d) - ac - bd = ad + bc
        var ac = x.A * y.A;
        var bd = x.B * y.B;
        var real = ac - bd;
        var imag = (x.A + x.B) * (y.A + y.B) - ac - bd;
        return new Fp2(real, imag);
    }

    /// <summary>
    /// Squaring: (a + bi)² = (a² - b²) + 2abi
    /// </summary>
    public Fp2 Square()
    {
        var a2 = A.Square();
        var b2 = B.Square();
        var ab2 = (A * B).Div2(); // Actually want 2ab, so compute (a*b) then double
        var twoAb = A * B + A * B; // 2ab
        return new Fp2(a2 - b2, twoAb);
    }

    public Fp2 Div2()
    {
        return new Fp2(A.Div2(), B.Div2());
    }

    /// <summary>
    /// Multiplicative inverse: 1/(a + bi) = (a - bi)/(a² + b²)
    /// </summary>
    public Fp2 Inverse()
    {
        // norm = a² + b²
        var norm = A.Square() + B.Square();
        var normInv = norm.Inverse();
        return new Fp2(A * normInv, -B * normInv);
    }

    /// <summary>
    /// Computes the square root in Fp2 if it exists.
    /// Uses the Tonelli-Shanks style algorithm adapted for Fp2.
    /// </summary>
    public Fp2? Sqrt()
    {
        if (A.IsZero && B.IsZero)
            return Zero;

        // For Fp2 with i² = -1, we need to find sqrt(a + bi)
        // Let sqrt(a + bi) = x + yi
        // Then x² - y² = a and 2xy = b

        var norm = A.Square() + B.Square();
        var normSqrt = norm.Sqrt();
        if (normSqrt == null)
            return null;

        // x² = (a + |z|) / 2 where |z| = sqrt(a² + b²)
        var xSquared = (A + normSqrt.Value).Div2();
        var x = xSquared.Sqrt();

        if (x == null)
        {
            // Try the other root
            xSquared = (A - normSqrt.Value).Div2();
            x = xSquared.Sqrt();
            if (x == null)
                return null;
        }

        Fp y;
        if (x.Value.IsZero)
        {
            // b = 0, handle specially
            var ySquared = (-A).Sqrt();
            if (ySquared == null)
                return null;
            y = ySquared.Value;
        }
        else
        {
            // y = b / (2x)
            var twoX = x.Value + x.Value;
            y = B * twoX.Inverse();
        }

        var candidate = new Fp2(x.Value, y);

        // Verify
        var squared = candidate.Square();
        if (squared == this)
            return candidate;

        // Try negating y
        candidate = new Fp2(x.Value, -y);
        squared = candidate.Square();
        if (squared == this)
            return candidate;

        // Try negating x
        candidate = new Fp2(-x.Value, -y);
        squared = candidate.Square();
        if (squared == this)
            return candidate;

        candidate = new Fp2(-x.Value, y);
        squared = candidate.Square();
        if (squared == this)
            return candidate;

        return null;
    }

    /// <summary>
    /// Converts to 32 bytes (little-endian, a first then b).
    /// </summary>
    public void ToBytes(Span<byte> output)
    {
        if (output.Length < 32)
            throw new ArgumentException("Output must be at least 32 bytes", nameof(output));

        A.ToBytes(output.Slice(0, 16));
        B.ToBytes(output.Slice(16, 16));
    }

    /// <summary>
    /// Creates an Fp2 from 32 bytes (little-endian).
    /// </summary>
    public static Fp2 FromBytes(ReadOnlySpan<byte> input)
    {
        if (input.Length < 32)
            throw new ArgumentException("Input must be at least 32 bytes", nameof(input));

        var a = Fp.FromBytes(input.Slice(0, 16));
        var b = Fp.FromBytes(input.Slice(16, 16));
        return new Fp2(a, b);
    }

    public bool Equals(Fp2 other) => A == other.A && B == other.B;
    public override bool Equals(object? obj) => obj is Fp2 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(A, B);
    public static bool operator ==(Fp2 left, Fp2 right) => left.Equals(right);
    public static bool operator !=(Fp2 left, Fp2 right) => !left.Equals(right);
    public override string ToString() => $"({A} + {B}i)";

    public bool IsZero => A.IsZero && B.IsZero;
}
