using Qubic.Bob;

namespace Qubic.Bob.Tests;

public class BobNodeStateTests
{
    [Fact]
    public void GetHttpUrl_ReturnsCorrectUrl()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        Assert.Equal("https://bob01.qubic.li/qubic", node.GetHttpUrl("/qubic"));
    }

    [Fact]
    public void GetHttpUrl_TrimsTrailingSlash()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li/" };

        Assert.Equal("https://bob01.qubic.li/qubic", node.GetHttpUrl("/qubic"));
    }

    [Fact]
    public void GetHttpUrl_CustomPath()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        Assert.Equal("https://bob01.qubic.li/custom/rpc", node.GetHttpUrl("/custom/rpc"));
    }

    [Fact]
    public void GetWebSocketUrl_HttpsToWss()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        var wsUrl = node.GetWebSocketUrl("/ws/qubic");

        Assert.StartsWith("wss://", wsUrl);
        Assert.Contains("bob01.qubic.li", wsUrl);
        Assert.EndsWith("/ws/qubic", wsUrl);
    }

    [Fact]
    public void GetWebSocketUrl_HttpToWs()
    {
        var node = new BobNodeState { BaseUrl = "http://localhost:40420" };

        var wsUrl = node.GetWebSocketUrl("/ws/qubic");

        Assert.StartsWith("ws://", wsUrl);
        Assert.Contains("localhost", wsUrl);
        Assert.Contains("40420", wsUrl);
        Assert.EndsWith("/ws/qubic", wsUrl);
    }

    [Fact]
    public void DefaultState_IsAvailable()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        Assert.True(node.IsAvailable);
        Assert.Equal(0, node.ConsecutiveFailures);
        Assert.Equal(0u, node.LastVerifyLoggingTick);
        Assert.Equal(0u, node.LastSeenNetworkTick);
    }

    [Fact]
    public void State_TracksFailures()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        node.ConsecutiveFailures = 3;
        node.IsAvailable = false;

        Assert.False(node.IsAvailable);
        Assert.Equal(3, node.ConsecutiveFailures);
    }

    [Fact]
    public void State_TracksSyncProgress()
    {
        var node = new BobNodeState { BaseUrl = "https://bob01.qubic.li" };

        node.LastVerifyLoggingTick = 22500000;
        node.LastSeenNetworkTick = 22500050;
        node.Latency = TimeSpan.FromMilliseconds(42);
        node.LastHealthCheckUtc = DateTime.UtcNow;

        Assert.Equal(22500000u, node.LastVerifyLoggingTick);
        Assert.Equal(22500050u, node.LastSeenNetworkTick);
        Assert.Equal(42, node.Latency.TotalMilliseconds);
    }
}
