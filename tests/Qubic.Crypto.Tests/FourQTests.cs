using System;
using System.Numerics;
using Qubic.Crypto.FourQ;
using Xunit;

namespace Qubic.Crypto.Tests;

public class FourQTests
{
    [Fact]
    public void Fp_BasicArithmetic_Works()
    {
        var a = new Fp(BigInteger.Parse("12345678901234567890"));
        var b = new Fp(BigInteger.Parse("98765432109876543210"));

        var sum = a + b;
        var diff = b - a;
        var prod = a * b;

        Assert.True(sum.Value > 0);
        Assert.True(diff.Value > 0);
        Assert.True(prod.Value > 0);
    }

    [Fact]
    public void Fp_ModularReduction_KeepsValuesInRange()
    {
        // Value larger than p should be reduced
        var large = new Fp(Fp.P + 100);
        Assert.True(large.Value < Fp.P);
        Assert.Equal(100, (int)large.Value);
    }

    [Fact]
    public void Fp_ModularReduction_PReducesToZero()
    {
        // P mod P should be 0 (regression: previously caused infinite loop)
        var p = new Fp(Fp.P);
        Assert.Equal(BigInteger.Zero, p.Value);
    }

    [Fact]
    public void Fp_ModularReduction_MultiplesOfP_ReduceToZero()
    {
        var twoP = new Fp(Fp.P * 2);
        Assert.Equal(BigInteger.Zero, twoP.Value);

        var threeP = new Fp(Fp.P * 3);
        Assert.Equal(BigInteger.Zero, threeP.Value);
    }

    [Fact]
    public void Fp_Div2_OfP_IsZero()
    {
        // Div2 of P should produce 0 (since P ≡ 0 mod P)
        // This was the exact trigger for the infinite loop bug
        var p = new Fp(Fp.P);
        var half = p.Div2();
        Assert.Equal(BigInteger.Zero, half.Value);
    }

    [Fact]
    public void Fp_Addition_ResultEqualsP_ReducesToZero()
    {
        // (P-1) + 1 = P should reduce to 0
        var a = new Fp(Fp.P - 1);
        var b = Fp.One;
        var sum = a + b;
        Assert.Equal(BigInteger.Zero, sum.Value);
    }

    [Fact]
    public void Fp_Inverse_Works()
    {
        var a = new Fp(BigInteger.Parse("42"));
        var aInv = a.Inverse();
        var product = a * aInv;

        Assert.Equal(Fp.One, product);
    }

    [Fact]
    public void Fp_Sqrt_Works()
    {
        // 4 is a quadratic residue
        var four = new Fp(4);
        var sqrt = four.Sqrt();

        Assert.NotNull(sqrt);
        Assert.Equal(four, sqrt.Value.Square());
    }

    [Fact]
    public void Fp2_BasicArithmetic_Works()
    {
        var a = new Fp2(new Fp(3), new Fp(4));
        var b = new Fp2(new Fp(1), new Fp(2));

        var sum = a + b;
        var diff = a - b;
        var prod = a * b;

        Assert.Equal(new Fp(4), sum.A);
        Assert.Equal(new Fp(6), sum.B);
    }

    [Fact]
    public void Fp2_Multiplication_FollowsComplexRules()
    {
        // (3 + 4i) * (1 + 2i) = 3 + 6i + 4i + 8i² = 3 + 10i - 8 = -5 + 10i
        var a = new Fp2(new Fp(3), new Fp(4));
        var b = new Fp2(new Fp(1), new Fp(2));
        var prod = a * b;

        // -5 mod p = p - 5
        var expectedReal = new Fp(Fp.P - 5);
        Assert.Equal(expectedReal, prod.A);
        Assert.Equal(new Fp(10), prod.B);
    }

    [Fact]
    public void Fp2_Inverse_Works()
    {
        var a = new Fp2(new Fp(3), new Fp(4));
        var aInv = a.Inverse();
        var product = a * aInv;

        Assert.Equal(Fp2.One, product);
    }

    [Fact]
    public void Point_Identity_IsOnCurve()
    {
        Assert.True(FourQPoint.IsOnCurve(PointExt.Identity));
    }

    [Fact]
    public void Point_BasePoint_IsOnCurve()
    {
        Assert.True(FourQPoint.IsOnCurve(FourQPoint.BasePoint));
    }

    [Fact]
    public void Point_DoubleIdentity_IsIdentity()
    {
        var doubled = FourQPoint.Double(PointExt.Identity);
        Assert.True(doubled.IsIdentity);
    }

    [Fact]
    public void Point_AddIdentity_ReturnsSamePoint()
    {
        var p = FourQPoint.BasePoint;
        var sum = FourQPoint.Add(p, PointExt.Identity);
        Assert.Equal(p, sum);
    }

    [Fact]
    public void Point_ScalarMulByZero_ReturnsIdentity()
    {
        var result = FourQPoint.ScalarMul(FourQPoint.BasePoint, BigInteger.Zero);
        Assert.True(result.IsIdentity);
    }

    [Fact]
    public void Point_ScalarMulByOne_ReturnsSamePoint()
    {
        var result = FourQPoint.ScalarMul(FourQPoint.BasePoint, BigInteger.One);
        Assert.Equal(FourQPoint.BasePoint, result);
    }

    [Fact]
    public void Point_ScalarMulByTwo_EqualsDouble()
    {
        var doubled = FourQPoint.Double(FourQPoint.BasePoint);
        var scalared = FourQPoint.ScalarMul(FourQPoint.BasePoint, 2);
        Assert.Equal(doubled, scalared);
    }

    [Fact]
    public void Point_ScalarMulResult_IsOnCurve()
    {
        var result = FourQPoint.ScalarMul(FourQPoint.BasePoint, 12345);
        Assert.True(FourQPoint.IsOnCurve(result));
    }

    [Fact]
    public void Point_EncodeAndDecode_RoundTrips()
    {
        var point = FourQPoint.ScalarMul(FourQPoint.BasePoint, 42);
        var encoded = FourQCodec.Encode(point);
        var decoded = FourQCodec.Decode(encoded);

        Assert.NotNull(decoded);
        Assert.Equal(point, decoded.Value);
    }

    [Fact]
    public void Point_BasePointEncodeDecode_Works()
    {
        var encoded = FourQCodec.Encode(FourQPoint.BasePoint);
        var decoded = FourQCodec.Decode(encoded);

        Assert.NotNull(decoded);
        Assert.Equal(FourQPoint.BasePoint, decoded.Value);
    }

    [Fact]
    public void Point_DecodeZeroBytes_DoesNotHang()
    {
        // Decoding an all-zero 32-byte public key should complete (not hang)
        // This triggered the Fp.Mod infinite loop before the fix
        var zeroKey = new byte[32];
        var decoded = FourQCodec.Decode(zeroKey);

        // The point may or may not be valid, but Decode must terminate
        if (decoded != null)
        {
            Assert.True(FourQPoint.IsOnCurve(decoded.Value));
        }
    }

    [Fact]
    public void ScalarField_Reduce_KeepsValuesInRange()
    {
        var large = ScalarField.N + 100;
        var reduced = ScalarField.Reduce(large);
        Assert.True(reduced < ScalarField.N);
        Assert.Equal(100, (int)reduced);
    }

    [Fact]
    public void ScalarField_FromBytes_AndToBytes_RoundTrips()
    {
        var bytes = new byte[32];
        bytes[0] = 0x42;
        bytes[1] = 0x13;

        var scalar = ScalarField.FromBytes32LE(bytes);
        var outputBytes = ScalarField.ToBytes32LE(scalar);

        Assert.Equal(bytes, outputBytes);
    }

    [Fact]
    public void ScalarField_MulAndAdd_Work()
    {
        var a = ScalarField.Reduce(12345);
        var b = ScalarField.Reduce(67890);

        var sum = ScalarField.Add(a, b);
        var prod = ScalarField.Mul(a, b);

        Assert.Equal(ScalarField.Reduce(12345 + 67890), sum);
        Assert.Equal(ScalarField.Reduce(12345 * 67890), prod);
    }
}
