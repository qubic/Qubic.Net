using Qubic.Bob;
using Qubic.Bob.Models;
using Xunit;

namespace Qubic.Bob.Tests;

/// <summary>
/// Integration tests that run against a real Bob node WebSocket.
///
/// These tests are skipped by default. To run them, set:
///   BOB_WS_URL=https://bob02.qubic.li
///
/// Run with: dotnet test --filter "Category=RealBobWs"
/// </summary>
[Trait("Category", "RealBobWs")]
public class BobWebSocketIntegrationTests : IAsyncDisposable
{
    private static readonly string? BobUrl = Environment.GetEnvironmentVariable("BOB_WS_URL");
    private static bool HasBobConfigured => !string.IsNullOrEmpty(BobUrl);

    private BobWebSocketClient? _client;

    private async Task<BobWebSocketClient> GetConnectedClientAsync()
    {
        if (_client is not null)
            return _client;

        var events = new List<BobConnectionEvent>();
        var options = new BobWebSocketOptions
        {
            Nodes = [BobUrl!],
            OnConnectionEvent = e => events.Add(e)
        };

        _client = new BobWebSocketClient(options);
        await _client.ConnectAsync();
        return _client;
    }

    private void SkipIfNoBob()
    {
        Skip.If(!HasBobConfigured,
            "Skipping Bob WebSocket test: Set BOB_WS_URL environment variable to run.");
    }

    #region Connection

    [SkippableFact]
    public async Task ConnectAsync_ConnectsSuccessfully()
    {
        SkipIfNoBob();

        var events = new List<BobConnectionEvent>();
        var options = new BobWebSocketOptions
        {
            Nodes = [BobUrl!],
            OnConnectionEvent = e => events.Add(e)
        };

        await using var client = new BobWebSocketClient(options);
        await client.ConnectAsync();

        Assert.Equal(BobConnectionState.Connected, client.State);
        Assert.NotNull(client.ActiveNodeUrl);
        Assert.Contains(events, e => e.Type == BobConnectionEventType.Connected);
    }

    #endregion

    #region Subscriptions

    [SkippableFact]
    public async Task SubscribeNewTicksAsync_ReceivesTicks()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        using var subscription = await client.SubscribeNewTicksAsync();

        Assert.NotNull(subscription.ServerSubscriptionId);

        // Read a few ticks — allow generous timeout since ~1 tick/second
        var ticks = new List<NewTickNotification>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await foreach (var tick in subscription.WithCancellation(cts.Token))
            {
                ticks.Add(tick);
                if (ticks.Count >= 3) break;
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout — assert on what we collected
        }

        Assert.True(ticks.Count >= 1, $"Expected at least 1 tick, got {ticks.Count}");
        Assert.All(ticks, t =>
        {
            Assert.True(t.TickNumber > 0);
            Assert.True(t.Epoch > 0);
        });

        // Ticks should be sequential
        for (int i = 1; i < ticks.Count; i++)
        {
            Assert.True(ticks[i].TickNumber >= ticks[i - 1].TickNumber,
                $"Tick {ticks[i].TickNumber} should be >= {ticks[i - 1].TickNumber}");
        }
    }

    [SkippableFact]
    public async Task SubscribeTickStreamAsync_ReceivesTickData()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        using var subscription = await client.SubscribeTickStreamAsync(new TickStreamOptions
        {
            SkipEmptyTicks = false,
            IncludeInputData = false
        });

        Assert.NotNull(subscription.ServerSubscriptionId);

        var ticks = new List<TickStreamNotification>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        try
        {
            await foreach (var tick in subscription.WithCancellation(cts.Token))
            {
                ticks.Add(tick);
                if (ticks.Count >= 2) break;
            }
        }
        catch (OperationCanceledException)
        {
            // Timeout — assert on what we collected
        }

        Assert.True(ticks.Count >= 1, $"Expected at least 1 tick, got {ticks.Count}");
        Assert.All(ticks, t =>
        {
            Assert.True(t.Tick > 0);
            Assert.True(t.Epoch > 0);
        });
    }

    [SkippableFact]
    public async Task SubscribeTransfersAsync_Succeeds()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        // Subscribe without identity filter — should receive all transfers
        using var subscription = await client.SubscribeTransfersAsync();

        Assert.NotNull(subscription.ServerSubscriptionId);

        // Just verify the subscription was accepted; transfers may not arrive immediately
        // Read for a short time to verify the stream works
        var items = new List<TransferNotification>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            await foreach (var item in subscription.WithCancellation(cts.Token))
            {
                items.Add(item);
                if (items.Count >= 1) break;
            }
        }
        catch (OperationCanceledException)
        {
            // Acceptable — may not receive transfers within timeout
        }

        // The subscription was created successfully regardless of whether data arrived
        Assert.NotNull(subscription.ServerSubscriptionId);
    }

    [SkippableFact]
    public async Task SubscribeLogsAsync_Succeeds()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        using var subscription = await client.SubscribeLogsAsync(new LogSubscriptionOptions
        {
            LogTypes = [0] // QU_TRANSFER logs
        });

        Assert.NotNull(subscription.ServerSubscriptionId);

        var items = new List<LogNotification>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        try
        {
            await foreach (var item in subscription.WithCancellation(cts.Token))
            {
                items.Add(item);
                if (items.Count >= 1) break;
            }
        }
        catch (OperationCanceledException)
        {
            // Acceptable — may not receive logs within timeout
        }

        Assert.NotNull(subscription.ServerSubscriptionId);
    }

    [SkippableFact]
    public async Task UnsubscribeAsync_RemovesSubscription()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var subscription = await client.SubscribeNewTicksAsync();
        Assert.NotNull(subscription.ServerSubscriptionId);

        await client.UnsubscribeAsync(subscription);

        Assert.True(subscription.CancellationToken.IsCancellationRequested);
    }

    #endregion

    #region Multi-Node Health

    [SkippableFact]
    public async Task ConnectAsync_WithMultipleNodes_SelectsBest()
    {
        SkipIfNoBob();

        var events = new List<BobConnectionEvent>();
        var options = new BobWebSocketOptions
        {
            // Use the same node twice to test multi-node path without needing multiple real nodes
            Nodes = [BobUrl!, BobUrl!],
            OnConnectionEvent = e => events.Add(e)
        };

        await using var client = new BobWebSocketClient(options);
        await client.ConnectAsync();

        Assert.Equal(BobConnectionState.Connected, client.State);
        Assert.Contains(events, e => e.Type == BobConnectionEventType.Connected);
    }

    #endregion

    #region RPC Queries over WebSocket

    [SkippableFact]
    public async Task GetChainIdAsync_ReturnsChainId()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var result = await client.GetChainIdAsync();

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.ChainId));
        Assert.False(string.IsNullOrEmpty(result.Network));
    }

    [SkippableFact]
    public async Task GetClientVersionAsync_ReturnsVersion()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var result = await client.GetClientVersionAsync();

        Assert.False(string.IsNullOrEmpty(result));
    }

    [SkippableFact]
    public async Task GetSyncingAsync_ReturnsSyncStatus()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var result = await client.GetSyncingAsync();

        Assert.NotNull(result);
        Assert.True(result.CurrentVerifyLoggingTick > 0 || result.CurrentFetchingTick > 0);
    }

    [SkippableFact]
    public async Task GetTickNumberAsync_ReturnsNonZero()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var tickNumber = await client.GetTickNumberAsync();

        Assert.True(tickNumber > 0);
    }

    [SkippableFact]
    public async Task GetCurrentEpochAsync_ReturnsEpoch()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var result = await client.GetCurrentEpochAsync();

        Assert.NotNull(result);
        Assert.True(result.Epoch > 0);
    }

    [SkippableFact]
    public async Task CallAsync_GenericWithArbitraryMethod_Works()
    {
        SkipIfNoBob();
        var client = await GetConnectedClientAsync();

        var result = await client.CallAsync<uint>("qubic_getTickNumber");

        Assert.True(result > 0);
    }

    #endregion

    public async ValueTask DisposeAsync()
    {
        if (_client is not null)
            await _client.DisposeAsync();
    }
}
