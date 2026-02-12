using System;
using Xunit;

namespace Qubic.Crypto.Tests;

public class K12Tests
{
    
    [Fact]
    public void K12_EmptyInput_ProducesValidHash()
    {
        var result = K12.Hash(ReadOnlySpan<byte>.Empty, 32);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void K12_MatchesTsSchnorrqVector()
    {
        // Input: 0x00-0x09 (bytes 0-9)
        // Expected output from noble/hashes K12
        var input = new byte[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09 };
        var expected = HexToBytes("e5ef1dd415a069d5c1ee20a731c271d751ec9f301bfb8a51acf27828d231e305");

        var result = K12.Hash(input, 32);
        var resultHex = BitConverter.ToString(result).Replace("-", "").ToLowerInvariant();
        var expectedHex = BitConverter.ToString(expected).Replace("-", "").ToLowerInvariant();

        Assert.Equal(expectedHex, resultHex);
    }

    [Fact]
    public void K12_VariableOutputLength_Works()
    {
        var input = new byte[] { 0x01, 0x02, 0x03 };

        var hash16 = K12.Hash(input, 16);
        var hash32 = K12.Hash(input, 32);
        var hash64 = K12.Hash(input, 64);

        Assert.Equal(16, hash16.Length);
        Assert.Equal(32, hash32.Length);
        Assert.Equal(64, hash64.Length);

        // First 16 bytes of hash32 should equal hash16
        Assert.Equal(hash16, hash32.AsSpan(0, 16).ToArray());

        // First 32 bytes of hash64 should equal hash32
        Assert.Equal(hash32, hash64.AsSpan(0, 32).ToArray());
    }

    [Fact]
    public void K12_DeterministicOutput()
    {
        var input = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };

        var hash1 = K12.Hash(input, 32);
        var hash2 = K12.Hash(input, 32);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void K12_DifferentInputsDifferentOutputs()
    {
        var input1 = new byte[] { 0x01, 0x02, 0x03 };
        var input2 = new byte[] { 0x01, 0x02, 0x04 };

        var hash1 = K12.Hash(input1, 32);
        var hash2 = K12.Hash(input2, 32);

        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void K12_LargeInput_Works()
    {
        // Test with input larger than rate (168 bytes)
        var input = new byte[1000];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var result = K12.Hash(input, 32);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void K12_WithCustomization_Works()
    {
        var input = new byte[] { 0x01, 0x02, 0x03 };
        var customization = new byte[] { 0xAB, 0xCD };

        var hashWithoutCustom = K12.Hash(input, 32);
        var hashWithCustom = K12.Hash(input, customization, 32);

        Assert.NotEqual(hashWithoutCustom, hashWithCustom);
    }

    [Fact]
    public void K12_MultiBlock_LargerThan8KB_Works()
    {
        // Test with input larger than 8192 bytes (chunk size)
        var input = new byte[10000];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var result = K12.Hash(input, 32);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void K12_MultiBlock_ExactlyTwoChunks_Works()
    {
        // Test with exactly 2 chunks (16384 bytes)
        var input = new byte[16384];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var result = K12.Hash(input, 32);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void K12_MultiBlock_ThreeChunks_Works()
    {
        // Test with 3 chunks (24576 bytes)
        var input = new byte[24576];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var result = K12.Hash(input, 32);
        Assert.NotNull(result);
        Assert.Equal(32, result.Length);
    }

    [Fact]
    public void K12_MultiBlock_Deterministic()
    {
        var input = new byte[10000];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var hash1 = K12.Hash(input, 32);
        var hash2 = K12.Hash(input, 32);

        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void K12_MultiBlock_DifferentFromSingleBlock()
    {
        // First 8000 bytes
        var smallInput = new byte[8000];
        for (int i = 0; i < smallInput.Length; i++)
            smallInput[i] = (byte)(i & 0xFF);

        // 10000 bytes (starts the same)
        var largeInput = new byte[10000];
        for (int i = 0; i < largeInput.Length; i++)
            largeInput[i] = (byte)(i & 0xFF);

        var smallHash = K12.Hash(smallInput, 32);
        var largeHash = K12.Hash(largeInput, 32);

        // They should be different
        Assert.NotEqual(smallHash, largeHash);
    }

    [Fact]
    public void K12_MultiBlock_BoundaryAt8192_Works()
    {
        // Test at exact boundary (8192 = single block limit)
        var exactBoundary = new byte[8192];
        for (int i = 0; i < exactBoundary.Length; i++)
            exactBoundary[i] = (byte)(i & 0xFF);

        // Just over boundary (8193 = triggers multi-block)
        var justOver = new byte[8193];
        for (int i = 0; i < justOver.Length; i++)
            justOver[i] = (byte)(i & 0xFF);

        var boundaryHash = K12.Hash(exactBoundary, 32);
        var overHash = K12.Hash(justOver, 32);

        Assert.NotNull(boundaryHash);
        Assert.NotNull(overHash);
        Assert.NotEqual(boundaryHash, overHash);
    }

    [Fact]
    public void K12_MultiBlock_WithCustomization_Works()
    {
        var input = new byte[10000];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var customization = new byte[] { 0xAB, 0xCD };

        var hashWithoutCustom = K12.Hash(input, 32);
        var hashWithCustom = K12.Hash(input, customization, 32);

        Assert.NotEqual(hashWithoutCustom, hashWithCustom);
    }

    [Fact]
    public void K12_MultiBlock_VariableOutputLength_Works()
    {
        var input = new byte[10000];
        for (int i = 0; i < input.Length; i++)
            input[i] = (byte)(i & 0xFF);

        var hash16 = K12.Hash(input, 16);
        var hash32 = K12.Hash(input, 32);
        var hash64 = K12.Hash(input, 64);

        Assert.Equal(16, hash16.Length);
        Assert.Equal(32, hash32.Length);
        Assert.Equal(64, hash64.Length);

        // First 16 bytes of hash32 should equal hash16
        Assert.Equal(hash16, hash32.AsSpan(0, 16).ToArray());

        // First 32 bytes of hash64 should equal hash32
        Assert.Equal(hash32, hash64.AsSpan(0, 32).ToArray());
    }

    private static byte[] HexToBytes(string hex)
    {
        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string must have even length");

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
        return bytes;
    }
}
