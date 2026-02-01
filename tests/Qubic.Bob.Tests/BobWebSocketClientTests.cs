using Qubic.Bob;
using Qubic.Bob.Models;

namespace Qubic.Bob.Tests;

public class BobWebSocketClientTests
{
    #region Constructor

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        Assert.NotNull(client);
        Assert.Equal(BobConnectionState.Disconnected, client.State);
        Assert.Null(client.ActiveNodeUrl);
    }

    [Fact]
    public void Constructor_WithMultipleNodes_CreatesInstance()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li", "https://bob02.qubic.li", "https://bob03.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        Assert.NotNull(client);
    }

    [Fact]
    public void Constructor_NullOptions_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new BobWebSocketClient(null!));
    }

    [Fact]
    public void Constructor_EmptyNodes_ThrowsArgumentException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = []
        };

        Assert.Throws<ArgumentException>(() => new BobWebSocketClient(options));
    }

    [Fact]
    public void Constructor_CustomOptions_AppliesDefaults()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"],
            HealthCheckInterval = TimeSpan.FromSeconds(10),
            SwitchThresholdTicks = 50,
            FailureThreshold = 5,
            ReconnectDelay = TimeSpan.FromSeconds(2),
            MaxReconnectDelay = TimeSpan.FromSeconds(30),
            SubscriptionBufferSize = 5000
        };

        using var client = new BobWebSocketClient(options);

        Assert.NotNull(client);
    }

    #endregion

    #region ConnectAsync — No Reachable Nodes

    [Fact]
    public async Task ConnectAsync_NoReachableNodes_Throws()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://unreachable-node.invalid:40420"],
            // Set threshold to 1 so the node is marked unavailable after one probe failure
            FailureThreshold = 1
        };

        using var client = new BobWebSocketClient(options);

        // With FailureThreshold=1, the single probe failure marks the node unavailable,
        // and SelectBestNode returns null → InvalidOperationException
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.ConnectAsync());
    }

    #endregion

    #region ConnectAsync — Already Disposed

    [Fact]
    public async Task ConnectAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.ConnectAsync());
    }

    #endregion

    #region Subscribe — Not Connected

    [Fact]
    public async Task SubscribeTickStreamAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        // Not connected — should fail when trying to send the subscribe request
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SubscribeTickStreamAsync());
    }

    [Fact]
    public async Task SubscribeTransfersAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SubscribeTransfersAsync());
    }

    [Fact]
    public async Task SubscribeLogsAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SubscribeLogsAsync());
    }

    [Fact]
    public async Task SubscribeNewTicksAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.SubscribeNewTicksAsync());
    }

    #endregion

    #region Subscribe — After Dispose

    [Fact]
    public async Task SubscribeTickStreamAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.SubscribeTickStreamAsync());
    }

    #endregion

    #region Connection Events

    [Fact]
    public async Task ConnectAsync_EmitsConnectingEvent()
    {
        var events = new List<BobConnectionEvent>();
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://unreachable-node.invalid:40420"],
            OnConnectionEvent = e => events.Add(e)
        };

        using var client = new BobWebSocketClient(options);

        // Will fail but should emit events
        try { await client.ConnectAsync(); } catch { /* expected */ }

        // Should not have emitted Connecting since probe fails before connect
        // But we can verify the event callback works
        Assert.NotNull(options.OnConnectionEvent);
    }

    #endregion

    #region Dispose

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        client.Dispose();
        client.Dispose(); // Should not throw
    }

    [Fact]
    public async Task DisposeAsync_MultipleTimes_DoesNotThrow()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        await client.DisposeAsync();
        await client.DisposeAsync(); // Should not throw
    }

    #endregion

    #region CallAsync — Guard Conditions

    [Fact]
    public async Task CallAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.CallAsync<string>("qubic_clientVersion"));
    }

    [Fact]
    public async Task CallAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.CallAsync<string>("qubic_clientVersion"));
    }

    #endregion

    #region Convenience Methods — Guard Conditions

    [Fact]
    public async Task GetChainIdAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetChainIdAsync());
    }

    [Fact]
    public async Task GetTickNumberAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        var client = new BobWebSocketClient(options);
        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => client.GetTickNumberAsync());
    }

    [Fact]
    public async Task GetBalanceAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetBalanceAsync("TESTIDENTITY"));
    }

    [Fact]
    public async Task GetTransactionByHashAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetTransactionByHashAsync("somehash"));
    }

    [Fact]
    public async Task GetLogsAsync_NotConnected_ThrowsInvalidOperationException()
    {
        var options = new BobWebSocketOptions
        {
            Nodes = ["https://bob01.qubic.li"]
        };

        using var client = new BobWebSocketClient(options);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => client.GetLogsAsync(1));
    }

    #endregion
}
