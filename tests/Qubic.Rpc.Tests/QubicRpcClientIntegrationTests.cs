using Qubic.Core;
using Qubic.Core.Entities;
using Qubic.Rpc.Models;
using Xunit;

namespace Qubic.Rpc.Tests;

/// <summary>
/// Integration tests that run against the real Qubic RPC API at rpc.qubic.org.
///
/// These tests are skipped by default. To run them, set:
///   QUBIC_RPC_URL=https://rpc.qubic.org
///
/// Run with: dotnet test --filter "Category=RealRpc"
/// </summary>
[Trait("Category", "RealRpc")]
public class QubicRpcClientIntegrationTests : IDisposable
{
    private static readonly string? RpcUrl = Environment.GetEnvironmentVariable("QUBIC_RPC_URL");
    private static bool HasRpcConfigured => !string.IsNullOrEmpty(RpcUrl);

    private readonly QubicRpcClient? _client;

    public QubicRpcClientIntegrationTests()
    {
        if (HasRpcConfigured)
            _client = new QubicRpcClient(RpcUrl!);
    }

    private void SkipIfNoRpc()
    {
        Skip.If(!HasRpcConfigured,
            "Skipping RPC integration test: Set QUBIC_RPC_URL environment variable to run.");
    }

    #region Live API

    [SkippableFact]
    public async Task GetTickInfoAsync_ReturnsValidTickInfo()
    {
        SkipIfNoRpc();

        var result = await _client!.GetTickInfoAsync();

        Assert.True(result.Tick > 0, "Tick should be greater than 0.");
        Assert.True(result.Epoch > 0, "Epoch should be greater than 0.");
        Assert.True(result.InitialTick > 0, "InitialTick should be greater than 0.");
        Assert.True(result.Tick >= result.InitialTick, "Tick should be >= InitialTick.");
    }

    [SkippableFact]
    public async Task GetBalanceAsync_ForZeroAddress_ReturnsBalance()
    {
        SkipIfNoRpc();

        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var result = await _client!.GetBalanceAsync(identity);

        Assert.Equal(identity, result.Identity);
        Assert.True(result.Amount >= 0, "Balance should be non-negative.");
        Assert.True(result.AtTick > 0, "AtTick should be set.");
    }

    [SkippableFact]
    public async Task GetBalanceAsync_ForArbitrator_ReturnsBalance()
    {
        SkipIfNoRpc();

        var identity = QubicIdentity.FromIdentity(QubicConstants.ArbitratorIdentity);
        var result = await _client!.GetBalanceAsync(identity);

        Assert.Equal(identity, result.Identity);
        Assert.True(result.Amount >= 0);
    }

    [SkippableFact]
    public async Task GetIssuedAssetsAsync_ForZeroAddress_ReturnsEmptyOrAssets()
    {
        SkipIfNoRpc();

        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var result = await _client!.GetIssuedAssetsAsync(identity);

        // Zero address likely has no issued assets, but the call should succeed
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task GetOwnedAssetsAsync_ForZeroAddress_Succeeds()
    {
        SkipIfNoRpc();

        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var result = await _client!.GetOwnedAssetsAsync(identity);

        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task GetPossessedAssetsAsync_ForZeroAddress_Succeeds()
    {
        SkipIfNoRpc();

        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var result = await _client!.GetPossessedAssetsAsync(identity);

        Assert.NotNull(result);
    }

    #endregion

    #region Query API

    [SkippableFact]
    public async Task GetLastProcessedTickAsync_ReturnsValidData()
    {
        SkipIfNoRpc();

        var result = await _client!.GetLastProcessedTickAsync();

        Assert.True(result.TickNumber > 0, "TickNumber should be greater than 0.");
        Assert.True(result.Epoch > 0, "Epoch should be greater than 0.");
    }

    [SkippableFact]
    public async Task GetProcessedTickIntervalsAsync_ReturnsAtLeastOneInterval()
    {
        SkipIfNoRpc();

        var result = await _client!.GetProcessedTickIntervalsAsync();

        Assert.NotEmpty(result);
        Assert.True(result[0].Epoch > 0);
        Assert.True(result[0].LastTick >= result[0].FirstTick);
    }

    [SkippableFact]
    public async Task GetTickDataAsync_ForRecentTick_ReturnsData()
    {
        SkipIfNoRpc();

        // Get last processed tick, then query its data
        var lastTick = await _client!.GetLastProcessedTickAsync();
        var result = await _client.GetTickDataAsync(lastTick.TickNumber);

        Assert.NotNull(result);
        Assert.Equal(lastTick.TickNumber, result.TickNumber);
        Assert.True(result.Epoch > 0);
    }

    [SkippableFact]
    public async Task GetTransactionsForTickAsync_ForRecentTick_Succeeds()
    {
        SkipIfNoRpc();

        var lastTick = await _client!.GetLastProcessedTickAsync();
        var result = await _client.GetTransactionsForTickAsync(lastTick.TickNumber);

        // A tick may have 0 or more transactions — just verify the call succeeds
        Assert.NotNull(result);
    }

    [SkippableFact]
    public async Task GetTransactionsForIdentityAsync_ForZeroAddress_ReturnsResult()
    {
        SkipIfNoRpc();

        var result = await _client!.GetTransactionsForIdentityAsync(new TransactionsForIdentityRequest
        {
            Identity = "BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID",
            Pagination = new PaginationOptions { Offset = 0, Size = 5 }
        });

        Assert.NotNull(result);
        Assert.NotNull(result.Hits);
        Assert.NotNull(result.Transactions);
    }

    [SkippableFact]
    public async Task GetComputorListsForEpochAsync_ForCurrentEpoch_Succeeds()
    {
        SkipIfNoRpc();

        var lastTick = await _client!.GetLastProcessedTickAsync();
        var result = await _client.GetComputorListsForEpochAsync(lastTick.Epoch);

        Assert.NotNull(result);
        // Current epoch should have at least one computor list
        Assert.NotEmpty(result);
        Assert.True(result[0].Identities.Count > 0);
    }

    #endregion

    #region Cross-API

    [SkippableFact]
    public async Task LiveAndQueryTicks_AreConsistent()
    {
        SkipIfNoRpc();

        var liveTickInfo = await _client!.GetTickInfoAsync();
        var lastProcessed = await _client.GetLastProcessedTickAsync();

        // Live tick should be >= archive's last processed tick
        Assert.True(liveTickInfo.Tick >= lastProcessed.TickNumber,
            $"Live tick ({liveTickInfo.Tick}) should be >= last processed ({lastProcessed.TickNumber}).");

        // Both should report the same epoch (or live is one ahead during epoch transition)
        Assert.True(liveTickInfo.Epoch >= lastProcessed.Epoch,
            $"Live epoch ({liveTickInfo.Epoch}) should be >= archive epoch ({lastProcessed.Epoch}).");
    }

    #endregion

    public void Dispose()
    {
        _client?.Dispose();
    }
}
