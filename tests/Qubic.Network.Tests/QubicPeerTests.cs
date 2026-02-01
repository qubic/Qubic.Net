using System.Net;
using Qubic.Network;

namespace Qubic.Network.Tests;

public class QubicPeerTests
{
    [Fact]
    public void Constructor_WithAddress_CreatesValidPeer()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Parse("192.168.1.100")
        };

        Assert.Equal(IPAddress.Parse("192.168.1.100"), peer.Address);
        Assert.Equal(21841, peer.Port); // Default port
        Assert.False(peer.IsReachable);
        Assert.Null(peer.LastSeen);
        Assert.Null(peer.CurrentTick);
    }

    [Fact]
    public void Constructor_WithCustomPort_SetsPort()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Parse("10.0.0.1"),
            Port = 12345
        };

        Assert.Equal(12345, peer.Port);
    }

    [Fact]
    public void IsReachable_CanBeSet()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Loopback,
            IsReachable = true
        };

        Assert.True(peer.IsReachable);

        peer.IsReachable = false;
        Assert.False(peer.IsReachable);
    }

    [Fact]
    public void LastSeen_CanBeSet()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Loopback
        };
        var now = DateTime.UtcNow;

        peer.LastSeen = now;

        Assert.Equal(now, peer.LastSeen);
    }

    [Fact]
    public void CurrentTick_CanBeSet()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Loopback
        };

        peer.CurrentTick = 12345678;

        Assert.Equal(12345678u, peer.CurrentTick);
    }

    [Fact]
    public void ToString_ReturnsAddressAndPort()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Parse("192.168.1.1"),
            Port = 21841
        };

        Assert.Equal("192.168.1.1:21841", peer.ToString());
    }

    [Fact]
    public void ToString_WithIPv6_ReturnsCorrectFormat()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.IPv6Loopback
        };

        Assert.Equal("::1:21841", peer.ToString());
    }

    [Fact]
    public void Constructor_WithLoopback_CreatesValidPeer()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Loopback
        };

        Assert.Equal(IPAddress.Loopback, peer.Address);
    }

    [Fact]
    public void Constructor_WithAny_CreatesValidPeer()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Any
        };

        Assert.Equal(IPAddress.Any, peer.Address);
    }

    [Fact]
    public void DefaultPort_Is21841()
    {
        var peer = new QubicPeer
        {
            Address = IPAddress.Loopback
        };

        Assert.Equal(21841, peer.Port);
    }

    [Fact]
    public void Properties_CanBeSetAtInitialization()
    {
        var lastSeen = DateTime.UtcNow;
        var peer = new QubicPeer
        {
            Address = IPAddress.Parse("8.8.8.8"),
            Port = 8080,
            IsReachable = true,
            LastSeen = lastSeen,
            CurrentTick = 99999
        };

        Assert.Equal(IPAddress.Parse("8.8.8.8"), peer.Address);
        Assert.Equal(8080, peer.Port);
        Assert.True(peer.IsReachable);
        Assert.Equal(lastSeen, peer.LastSeen);
        Assert.Equal(99999u, peer.CurrentTick);
    }
}
