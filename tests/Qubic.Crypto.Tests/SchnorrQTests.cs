using System;
using Xunit;

namespace Qubic.Crypto.Tests;

public class SchnorrQTests
{
    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < bytes.Length; i++)
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        return bytes;
    }

    private static string BytesToHex(byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

    [Fact]
    public void GeneratePublicKey_MatchesTsSchnorrqVector()
    {
        var subSeed = HexToBytes("179c2d6db171f1af86efbaded25aeb0bc12bf90c0244566aee23e08f082d3863");
        var expectedPublicKey = HexToBytes("39665f596c87c5eb34b7c2027ea87737dc5a178dda273a1e67d673e5925b4e82");

        var publicKey = SchnorrQ.GeneratePublicKey(subSeed);

        // Debug: Output actual vs expected
        var actualHex = BytesToHex(publicKey);
        var expectedHex = BytesToHex(expectedPublicKey);

        Assert.Equal(expectedHex, actualHex);
    }

    [Fact]
    public void K12_DebugHash()
    {
        // Test K12 hash of subSeed to 64 bytes
        var subSeed = HexToBytes("179c2d6db171f1af86efbaded25aeb0bc12bf90c0244566aee23e08f082d3863");
        var hash = K12.Hash(subSeed, 64);
        var hashHex = BytesToHex(hash);

        Assert.NotNull(hash);
    }

    [Fact]
    public void Sign_MatchesTsSchnorrqVector()
    {
        var subSeed = HexToBytes("179c2d6db171f1af86efbaded25aeb0bc12bf90c0244566aee23e08f082d3863");
        var publicKey = HexToBytes("39665f596c87c5eb34b7c2027ea87737dc5a178dda273a1e67d673e5925b4e82");
        var digest = HexToBytes("7f80905b2aee289f448193827a0a96ecc99de08f446b71c0f581255403778904");
        var expectedSignature = HexToBytes("ee65334ebf9a12407b85ae25d2e9eb0ff634fddc8e8bf2aab9ecd2a80fefca164b3fd48fbae7767e8f17bb794dd9d1698d03ee04db642546cd491ba76bd72700");

        var signature = SchnorrQ.Sign(subSeed, publicKey, digest);

        Assert.Equal(expectedSignature, signature);
    }

    [Fact]
    public void Verify_MatchesTsSchnorrqVector()
    {
        var publicKey = HexToBytes("39665f596c87c5eb34b7c2027ea87737dc5a178dda273a1e67d673e5925b4e82");
        var digest = HexToBytes("7f80905b2aee289f448193827a0a96ecc99de08f446b71c0f581255403778904");
        var signature = HexToBytes("ee65334ebf9a12407b85ae25d2e9eb0ff634fddc8e8bf2aab9ecd2a80fefca164b3fd48fbae7767e8f17bb794dd9d1698d03ee04db642546cd491ba76bd72700");

        var isValid = SchnorrQ.Verify(publicKey, digest, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_QubicNetworkSignedPacket()
    {
        // Real Qubic network test case
        var identity = "UGQLSPXWWQORKDDJNOQVYRPYPWKDYLBCTOJCQTPRJFUXGTQXJAVACKSDDNMA";
        var publicKey = GetPublicKeyFromIdentity(identity);
        var signedPacket = Convert.FromBase64String("SGFsbG9w3PrF3AvP1/epGGEbt79ZtwuDUP1UrxUKQSxw8Un31EKICNOIoqmuC9W/52M8Xg5islHGdAuPwOCS3OBjHwgA");

        // The signature is the last 64 bytes
        var message = signedPacket.AsSpan(0, signedPacket.Length - 64);
        var signature = signedPacket.AsSpan(signedPacket.Length - 64, 64);

        // Compute digest of message (everything except signature)
        var digest = K12.Hash(message, 32);

        var isValid = SchnorrQ.Verify(publicKey, digest, signature);

        Assert.True(isValid, "Invalid Signature - Qubic network verification failed");
    }

    [Fact]
    public void Identity_RoundTrip_PublicKeyToIdentityAndBack()
    {
        // Test that we can convert identity -> publicKey -> identity
        var originalIdentity = "UGQLSPXWWQORKDDJNOQVYRPYPWKDYLBCTOJCQTPRJFUXGTQXJAVACKSDDNMA";

        // Convert identity to public key
        var publicKey = GetPublicKeyFromIdentity(originalIdentity);

        // Convert public key back to identity
        var reconstructedIdentity = GetIdentityFromPublicKey(publicKey);

        Assert.Equal(originalIdentity, reconstructedIdentity);
    }

    [Fact]
    public void Identity_ChecksumValidation()
    {
        // The last 4 characters are a checksum derived from K12 hash of public key
        var identity = "UGQLSPXWWQORKDDJNOQVYRPYPWKDYLBCTOJCQTPRJFUXGTQXJAVACKSDDNMA";
        var publicKey = GetPublicKeyFromIdentity(identity);

        // Compute expected checksum
        var checksumBytes = K12.Hash(publicKey, 3);
        uint checksum = (uint)(checksumBytes[0] | (checksumBytes[1] << 8) | (checksumBytes[2] << 16));
        checksum &= 0x3FFFF; // 18 bits

        // Extract checksum characters from identity
        var checksumChars = new char[4];
        for (int i = 0; i < 4; i++)
        {
            checksumChars[i] = (char)('A' + (checksum % 26));
            checksum /= 26;
        }

        var expectedChecksum = new string(checksumChars);
        var actualChecksum = identity.Substring(56, 4);

        Assert.Equal(expectedChecksum, actualChecksum);
    }

    private static byte[] GetPublicKeyFromIdentity(string identity)
    {
        if (identity == null) throw new ArgumentException("Identity must not be null");
        if (identity.Length < 56) throw new ArgumentException("Identity must be 56-60 chars");
        if (identity.Length > 60) throw new ArgumentException("Identity must be 56-60 chars");

        byte[] buffer = new byte[32];

        for (int i = 0; i < 4; i++)
        {
            ulong value = 0;

            for (int j = 13; j >= 0; j--)
            {
                char c = identity[i * 14 + j]; // only uses first 56 chars
                if (c < 'A' || c > 'Z')
                    throw new ArgumentException("Invalid Identity [A-Z]");

                value = value * 26UL + (ulong)(c - 'A');
            }

            int offset = i * 8;
            buffer[offset + 0] = (byte)(value >> 0);
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
            buffer[offset + 4] = (byte)(value >> 32);
            buffer[offset + 5] = (byte)(value >> 40);
            buffer[offset + 6] = (byte)(value >> 48);
            buffer[offset + 7] = (byte)(value >> 56);
        }

        return buffer;
    }

    private static string GetIdentityFromPublicKey(byte[] publicKey, bool isLowerCase = false)
    {
        if (publicKey == null || publicKey.Length != 32)
            throw new ArgumentException("Public key must be 32 bytes");

        var identity = new char[60];

        // Convert each 8-byte chunk to 14 base-26 characters
        for (int i = 0; i < 4; i++)
        {
            ulong publicKeyFragment = BitConverter.ToUInt64(publicKey, i * 8);
            for (int j = 0; j < 14; j++)
            {
                identity[i * 14 + j] = (char)((publicKeyFragment % 26) + (isLowerCase ? 'a' : 'A'));
                publicKeyFragment /= 26;
            }
        }

        // Compute checksum: K12 hash of public key, take 3 bytes (18 bits used)
        var checksumBytes = K12.Hash(publicKey, 3);
        uint checksum = (uint)(checksumBytes[0] | (checksumBytes[1] << 8) | (checksumBytes[2] << 16));
        checksum &= 0x3FFFF; // 18 bits

        // Convert checksum to 4 base-26 characters
        for (int i = 0; i < 4; i++)
        {
            identity[56 + i] = (char)((checksum % 26) + (isLowerCase ? 'a' : 'A'));
            checksum /= 26;
        }

        return new string(identity);
    }

    [Fact]
    public void GetSubSeedFromSeed_ValidSeed_Returns32Bytes()
    {
        // 55 lowercase letters
        var seed = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc";

        var subSeed = SchnorrQ.GetSubSeedFromSeed(seed);

        Assert.NotNull(subSeed);
        Assert.Equal(32, subSeed.Length);
    }

    [Fact]
    public void GetSubSeedFromSeed_DeterministicForSameSeed()
    {
        var seed = "jvhbyzjinlyutyuhsweuxiwootqoevjqwqmdhjeohrytxjxidpbcfyg";

        var subSeed1 = SchnorrQ.GetSubSeedFromSeed(seed);
        var subSeed2 = SchnorrQ.GetSubSeedFromSeed(seed);

        Assert.Equal(subSeed1, subSeed2);
    }

    [Fact]
    public void GetSubSeedFromSeed_ThrowsOnInvalidSeed()
    {
        // Too short
        Assert.Throws<ArgumentException>(() => SchnorrQ.GetSubSeedFromSeed("abc"));

        // Too long
        Assert.Throws<ArgumentException>(() => SchnorrQ.GetSubSeedFromSeed(new string('a', 60)));

        // Contains uppercase
        Assert.Throws<ArgumentException>(() => SchnorrQ.GetSubSeedFromSeed("Abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc"));

        // Contains numbers
        Assert.Throws<ArgumentException>(() => SchnorrQ.GetSubSeedFromSeed("1bcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc"));

        // Null
        Assert.Throws<ArgumentException>(() => SchnorrQ.GetSubSeedFromSeed(null!));
    }

    [Fact]
    public void TryGetSubSeedFromSeed_ReturnsFalseOnInvalidSeed()
    {
        Assert.False(SchnorrQ.TryGetSubSeedFromSeed("abc", out _));
        Assert.False(SchnorrQ.TryGetSubSeedFromSeed("ABC", out _));
        Assert.False(SchnorrQ.TryGetSubSeedFromSeed(null!, out _));
    }

    [Fact]
    public void TryGetSubSeedFromSeed_ReturnsTrueOnValidSeed()
    {
        var seed = "jvhbyzjinlyutyuhsweuxiwootqoevjqwqmdhjeohrytxjxidpbcfyg";

        var result = SchnorrQ.TryGetSubSeedFromSeed(seed, out var subSeed);

        Assert.True(result);
        Assert.NotNull(subSeed);
        Assert.Equal(32, subSeed.Length);
    }

    [Fact]
    public void GeneratePrivateKey_FromSeed_Produces32Bytes()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var privateKey = SchnorrQ.GeneratePrivateKey(seed);

        Assert.NotNull(privateKey);
        Assert.Equal(32, privateKey.Length);
    }

    [Fact]
    public void GeneratePrivateKey_DeterministicForSameSeed()
    {
        var seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var pk1 = SchnorrQ.GeneratePrivateKey(seed);
        var pk2 = SchnorrQ.GeneratePrivateKey(seed);

        Assert.Equal(pk1, pk2);
    }

    [Fact]
    public void GeneratePublicKey_FromSeed_Produces32Bytes()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);

        Assert.NotNull(publicKey);
        Assert.Equal(32, publicKey.Length);
    }

    [Fact]
    public void GeneratePublicKey_DeterministicForSameSeed()
    {
        var seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var pk1 = SchnorrQ.GeneratePublicKey(seed);
        var pk2 = SchnorrQ.GeneratePublicKey(seed);

        Assert.Equal(pk1, pk2);
    }

    [Fact]
    public void GeneratePublicKey_DifferentSeedsProduceDifferentKeys()
    {
        var seed1 = new byte[32];
        var seed2 = new byte[32];
        seed1[0] = 0x01;
        seed2[0] = 0x02;

        var pk1 = SchnorrQ.GeneratePublicKey(seed1);
        var pk2 = SchnorrQ.GeneratePublicKey(seed2);

        Assert.NotEqual(pk1, pk2);
    }

    [Fact]
    public void Sign_ProducesValidSignature()
    {
        var seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)i;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var message = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var digest = SchnorrQ.Digest(message);

        var signature = SchnorrQ.Sign(seed, publicKey, digest);

        Assert.NotNull(signature);
        Assert.Equal(64, signature.Length);
    }

    [Fact]
    public void Sign_DeterministicForSameInputs()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var digest = new byte[32];
        digest[0] = 0xAB;

        var sig1 = SchnorrQ.Sign(seed, publicKey, digest);
        var sig2 = SchnorrQ.Sign(seed, publicKey, digest);

        Assert.Equal(sig1, sig2);
    }

    [Fact]
    public void Verify_AcceptsValidSignature()
    {
        var seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)(i + 1);

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var message = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
        var digest = SchnorrQ.Digest(message);
        var signature = SchnorrQ.Sign(seed, publicKey, digest);

        var isValid = SchnorrQ.Verify(publicKey, digest, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void Verify_RejectsModifiedSignature()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var digest = new byte[32];
        digest[0] = 0xAB;
        var signature = SchnorrQ.Sign(seed, publicKey, digest);

        // Modify signature
        signature[0] ^= 0xFF;

        var isValid = SchnorrQ.Verify(publicKey, digest, signature);

        Assert.False(isValid);
    }

    [Fact]
    public void Verify_RejectsWrongPublicKey()
    {
        var seed1 = new byte[32];
        var seed2 = new byte[32];
        seed1[0] = 0x01;
        seed2[0] = 0x02;

        var publicKey1 = SchnorrQ.GeneratePublicKey(seed1);
        var publicKey2 = SchnorrQ.GeneratePublicKey(seed2);
        var digest = new byte[32];
        var signature = SchnorrQ.Sign(seed1, publicKey1, digest);

        // Verify with wrong public key
        var isValid = SchnorrQ.Verify(publicKey2, digest, signature);

        Assert.False(isValid);
    }

    [Fact]
    public void Verify_RejectsWrongMessage()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var digest1 = new byte[32];
        var digest2 = new byte[32];
        digest1[0] = 0x01;
        digest2[0] = 0x02;

        var signature = SchnorrQ.Sign(seed, publicKey, digest1);

        // Verify with wrong message
        var isValid = SchnorrQ.Verify(publicKey, digest2, signature);

        Assert.False(isValid);
    }

    [Fact]
    public void SignMessage_AndVerifyMessage_Work()
    {
        var seed = new byte[32];
        for (int i = 0; i < 32; i++)
            seed[i] = (byte)(i * 2);

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var message = System.Text.Encoding.UTF8.GetBytes("Hello, Qubic!");

        var signature = SchnorrQ.SignMessage(seed, publicKey, message);
        var isValid = SchnorrQ.VerifyMessage(publicKey, message, signature);

        Assert.True(isValid);
    }

    [Fact]
    public void VerifyMessage_RejectsModifiedMessage()
    {
        var seed = new byte[32];
        seed[0] = 0x42;

        var publicKey = SchnorrQ.GeneratePublicKey(seed);
        var message1 = System.Text.Encoding.UTF8.GetBytes("Original message");
        var message2 = System.Text.Encoding.UTF8.GetBytes("Modified message");

        var signature = SchnorrQ.SignMessage(seed, publicKey, message1);
        var isValid = SchnorrQ.VerifyMessage(publicKey, message2, signature);

        Assert.False(isValid);
    }

    [Fact]
    public void Digest_ProducesConsistentOutput()
    {
        var message = new byte[] { 0x01, 0x02, 0x03 };

        var digest1 = SchnorrQ.Digest(message);
        var digest2 = SchnorrQ.Digest(message);

        Assert.Equal(digest1, digest2);
        Assert.Equal(32, digest1.Length);
    }

    [Fact]
    public void GeneratePublicKey_ThrowsOnWrongSeedSize()
    {
        var shortSeed = new byte[16];
        var longSeed = new byte[64];

        Assert.Throws<ArgumentException>(() => SchnorrQ.GeneratePublicKey(shortSeed));
        Assert.Throws<ArgumentException>(() => SchnorrQ.GeneratePublicKey(longSeed));
    }

    [Fact]
    public void Sign_ThrowsOnWrongInputSizes()
    {
        var validSeed = new byte[32];
        var validPk = new byte[32];
        var validDigest = new byte[32];
        var invalid = new byte[16];

        Assert.Throws<ArgumentException>(() => SchnorrQ.Sign(invalid, validPk, validDigest));
        Assert.Throws<ArgumentException>(() => SchnorrQ.Sign(validSeed, invalid, validDigest));
        Assert.Throws<ArgumentException>(() => SchnorrQ.Sign(validSeed, validPk, invalid));
    }

    [Fact]
    public void Verify_ReturnsFalseOnWrongInputSizes()
    {
        var valid32 = new byte[32];
        var valid64 = new byte[64];
        var invalid = new byte[16];

        Assert.False(SchnorrQ.Verify(invalid, valid32, valid64));
        Assert.False(SchnorrQ.Verify(valid32, invalid, valid64));
        Assert.False(SchnorrQ.Verify(valid32, valid32, invalid));
    }

    [Fact]
    public void MultipleSignVerify_AllSucceed()
    {
        // Test multiple sign/verify cycles to ensure consistency
        for (int i = 0; i < 10; i++)
        {
            var seed = new byte[32];
            seed[0] = (byte)i;
            seed[31] = (byte)(i * 7);

            var publicKey = SchnorrQ.GeneratePublicKey(seed);
            var message = new byte[100];
            for (int j = 0; j < message.Length; j++)
                message[j] = (byte)((i + j) % 256);

            var signature = SchnorrQ.SignMessage(seed, publicKey, message);
            var isValid = SchnorrQ.VerifyMessage(publicKey, message, signature);

            Assert.True(isValid, $"Verification failed for iteration {i}");
        }
    }
}
