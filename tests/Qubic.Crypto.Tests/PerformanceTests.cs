using System;
using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;

namespace Qubic.Crypto.Tests;

public class PerformanceTests
{
    private readonly ITestOutputHelper _output;
    private const string TestSeed = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc";

    public PerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void Benchmark_GetPublicKey()
    {
        var crypt = new QubicCrypt();

        // Warmup
        for (int i = 0; i < 5; i++)
            crypt.GetPublicKey(TestSeed);

        // Measure
        int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.GetPublicKey(TestSeed);
        sw.Stop();

        double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"GetPublicKey: {msPerOp:F2} ms/op, {opsPerSec:F0} ops/sec ({iterations} iterations in {sw.ElapsedMilliseconds} ms)");
    }

    [Fact]
    public void Benchmark_GetIdentityFromSeed_String()
    {
        var crypt = new QubicCrypt();

        // Warmup
        for (int i = 0; i < 5; i++)
            crypt.GetIdentityFromSeed(TestSeed);

        // Measure
        int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.GetIdentityFromSeed(TestSeed);
        sw.Stop();

        double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"GetIdentityFromSeed(string): {msPerOp:F2} ms/op, {opsPerSec:F0} ops/sec ({iterations} iterations in {sw.ElapsedMilliseconds} ms)");
    }

    [Fact]
    public void Benchmark_GetIdentityFromSeed_SpanOverload()
    {
        var crypt = new QubicCrypt();
        Span<char> identity = stackalloc char[60];

        // Warmup
        for (int i = 0; i < 5; i++)
            crypt.GetIdentityFromSeed(TestSeed, identity);

        // Measure
        int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.GetIdentityFromSeed(TestSeed, identity);
        sw.Stop();

        double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"GetIdentityFromSeed(Span): {msPerOp:F2} ms/op, {opsPerSec:F0} ops/sec ({iterations} iterations in {sw.ElapsedMilliseconds} ms)");
    }

    [Fact]
    public void Benchmark_GetIdentityFromSeed_FullyZeroAlloc()
    {
        var crypt = new QubicCrypt();
        Span<char> identity = stackalloc char[60];
        ReadOnlySpan<char> seed = TestSeed.AsSpan();

        // Warmup
        for (int i = 0; i < 5; i++)
            crypt.GetIdentityFromSeed(seed, identity);

        // Measure
        int iterations = 100;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.GetIdentityFromSeed(seed, identity);
        sw.Stop();

        double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"GetIdentityFromSeed(ReadOnlySpan, Span): {msPerOp:F2} ms/op, {opsPerSec:F0} ops/sec ({iterations} iterations in {sw.ElapsedMilliseconds} ms)");
    }

    [Fact]
    public void Benchmark_VanitySearchSimulation()
    {
        var crypt = new QubicCrypt();
        Span<char> identity = stackalloc char[60];
        var seedChars = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc".ToCharArray();
        int found = 0;

        // Warmup
        for (int i = 0; i < 5; i++)
        {
            seedChars[54] = (char)('a' + (i % 26));
            crypt.GetIdentityFromSeed(seedChars, identity);
        }

        // Simulate a vanity search: mutate last char, generate identity, check prefix
        int iterations = 500;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
        {
            seedChars[54] = (char)('a' + (i % 26));
            seedChars[53] = (char)('a' + ((i / 26) % 26));
            crypt.GetIdentityFromSeed(seedChars, identity);

            // Simulated prefix check
            if (identity[0] == 'Q')
                found++;
        }
        sw.Stop();

        double msPerOp = sw.Elapsed.TotalMilliseconds / iterations;
        double opsPerSec = iterations / sw.Elapsed.TotalSeconds;

        _output.WriteLine($"Vanity search simulation: {msPerOp:F2} ms/op, {opsPerSec:F0} ops/sec ({iterations} iterations in {sw.ElapsedMilliseconds} ms, {found} matches)");
    }

    [Fact]
    public void Benchmark_SignAndVerify()
    {
        var crypt = new QubicCrypt();
        var message = new byte[256];
        Random.Shared.NextBytes(message);

        // Warmup
        var sig = crypt.Sign(TestSeed, message);
        var pubKey = crypt.GetPublicKey(TestSeed);
        crypt.Verify(pubKey, message, sig);

        // Measure Sign
        int iterations = 50;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.Sign(TestSeed, message);
        sw.Stop();

        double signMs = sw.Elapsed.TotalMilliseconds / iterations;
        _output.WriteLine($"Sign: {signMs:F2} ms/op, {iterations / sw.Elapsed.TotalSeconds:F0} ops/sec");

        // Measure Verify
        sw.Restart();
        for (int i = 0; i < iterations; i++)
            crypt.Verify(pubKey, message, sig);
        sw.Stop();

        double verifyMs = sw.Elapsed.TotalMilliseconds / iterations;
        _output.WriteLine($"Verify: {verifyMs:F2} ms/op, {iterations / sw.Elapsed.TotalSeconds:F0} ops/sec");
    }

    [Fact]
    public void Benchmark_GetIdentityFromPublicKey()
    {
        var crypt = new QubicCrypt();
        var pubKey = crypt.GetPublicKey(TestSeed);

        // Warmup
        for (int i = 0; i < 100; i++)
            crypt.GetIdentityFromPublicKey(pubKey);

        // Measure — this is cheap (no EC math), just encoding
        int iterations = 10_000;
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < iterations; i++)
            crypt.GetIdentityFromPublicKey(pubKey);
        sw.Stop();

        double usPerOp = sw.Elapsed.TotalMicroseconds / iterations;
        _output.WriteLine($"GetIdentityFromPublicKey: {usPerOp:F1} us/op, {iterations / sw.Elapsed.TotalSeconds:F0} ops/sec");
    }

    [Fact]
    public void Benchmark_MultithreadedVanitySearch()
    {
        var crypt = new QubicCrypt();
        int totalOps = 0;
        int threadCount = Environment.ProcessorCount;

        // Warmup
        crypt.GetIdentityFromSeed(TestSeed);

        var sw = Stopwatch.StartNew();
        int targetPerThread = 100;

        var threads = new Thread[threadCount];
        for (int t = 0; t < threadCount; t++)
        {
            int threadId = t;
            threads[t] = new Thread(() =>
            {
                var localCrypt = new QubicCrypt();
                Span<char> identity = stackalloc char[60];
                var baseSeed = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabc".ToCharArray();
                baseSeed[0] = (char)('a' + (threadId % 26));
                var seedChars = baseSeed;

                for (int i = 0; i < targetPerThread; i++)
                {
                    seedChars[54] = (char)('a' + (i % 26));
                    seedChars[53] = (char)('a' + ((i / 26) % 26));
                    localCrypt.GetIdentityFromSeed(seedChars, identity);
                }

                Interlocked.Add(ref totalOps, targetPerThread);
            });
        }

        foreach (var t in threads) t.Start();
        foreach (var t in threads) t.Join();
        sw.Stop();

        double opsPerSec = totalOps / sw.Elapsed.TotalSeconds;
        _output.WriteLine($"Multithreaded ({threadCount} threads): {opsPerSec:F0} ops/sec total, {opsPerSec / threadCount:F0} ops/sec/thread ({totalOps} ops in {sw.ElapsedMilliseconds} ms)");
    }
}
