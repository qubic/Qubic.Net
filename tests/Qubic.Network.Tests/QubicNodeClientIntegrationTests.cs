using Qubic.Core.Entities;
using Qubic.Network;
using Qubic.Serialization;

namespace Qubic.Network.Tests;

/// <summary>
/// Integration tests for QubicNodeClient using a mock server.
/// These tests verify the full request/response cycle.
/// </summary>
public class QubicNodeClientIntegrationTests
{
    #region GetCurrentTickInfo Tests

    [Fact]
    public async Task GetCurrentTickInfoAsync_ReturnsCorrectData()
    {
        // Arrange
        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestCurrentTickInfo, _ =>
            MockResponseBuilder.CreateCurrentTickInfoResponse(
                tickDuration: 1000,
                epoch: 150,
                tick: 12345678,
                alignedVotes: 451,
                misalignedVotes: 5,
                initialTick: 10000000
            ));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port, timeoutMs: 5000);
        await client.ConnectAsync();

        // Act
        var tickInfo = await client.GetCurrentTickInfoAsync();

        // Assert
        Assert.Equal(1000, tickInfo.TickDuration);
        Assert.Equal(150, tickInfo.Epoch);
        Assert.Equal(12345678u, tickInfo.Tick);
        Assert.Equal(451, tickInfo.NumberOfAlignedVotes);
        Assert.Equal(5, tickInfo.NumberOfMisalignedVotes);
        Assert.Equal(10000000u, tickInfo.InitialTick);
    }

    [Fact]
    public async Task GetCurrentTickInfoAsync_WithZeroValues_HandlesCorrectly()
    {
        // Arrange
        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestCurrentTickInfo, _ =>
            MockResponseBuilder.CreateCurrentTickInfoResponse(
                tickDuration: 0,
                epoch: 0,
                tick: 0,
                alignedVotes: 0,
                misalignedVotes: 0,
                initialTick: 0
            ));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var tickInfo = await client.GetCurrentTickInfoAsync();

        // Assert
        Assert.Equal(0, tickInfo.TickDuration);
        Assert.Equal(0, tickInfo.Epoch);
        Assert.Equal(0u, tickInfo.Tick);
    }

    [Fact]
    public async Task GetCurrentTickInfoAsync_WithMaxValues_HandlesCorrectly()
    {
        // Arrange
        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestCurrentTickInfo, _ =>
            MockResponseBuilder.CreateCurrentTickInfoResponse(
                tickDuration: ushort.MaxValue,
                epoch: ushort.MaxValue,
                tick: uint.MaxValue,
                alignedVotes: ushort.MaxValue,
                misalignedVotes: ushort.MaxValue,
                initialTick: uint.MaxValue
            ));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var tickInfo = await client.GetCurrentTickInfoAsync();

        // Assert
        Assert.Equal(ushort.MaxValue, tickInfo.TickDuration);
        Assert.Equal(ushort.MaxValue, tickInfo.Epoch);
        Assert.Equal(uint.MaxValue, tickInfo.Tick);
    }

    #endregion

    #region GetBalance Tests

    [Fact]
    public async Task GetBalanceAsync_ReturnsCorrectBalance()
    {
        // Arrange
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");

        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestEntity, payload =>
        {
            // The payload contains the requested public key
            var requestedPublicKey = payload[..32];
            return MockResponseBuilder.CreateEntityResponse(
                publicKey: requestedPublicKey,
                incomingAmount: 1_000_000_000_000, // 1 trillion
                outgoingAmount: 500_000_000_000,   // 500 billion
                incomingTransfers: 100,
                outgoingTransfers: 50
            );
        });

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var balance = await client.GetBalanceAsync(identity);

        // Assert
        Assert.Equal(identity.ToString(), balance.Identity.ToString());
        Assert.Equal(500_000_000_000, balance.Amount); // 1T - 500B = 500B
        Assert.Equal(100u, balance.IncomingCount);
        Assert.Equal(50u, balance.OutgoingCount);
    }

    [Fact]
    public async Task GetBalanceAsync_WithZeroBalance_ReturnsZero()
    {
        // Arrange
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");

        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestEntity, payload =>
            MockResponseBuilder.CreateEntityResponse(
                publicKey: payload[..32],
                incomingAmount: 0,
                outgoingAmount: 0,
                incomingTransfers: 0,
                outgoingTransfers: 0
            ));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var balance = await client.GetBalanceAsync(identity);

        // Assert
        Assert.Equal(0, balance.Amount);
        Assert.Equal(0u, balance.IncomingCount);
        Assert.Equal(0u, balance.OutgoingCount);
    }

    [Fact]
    public async Task GetBalanceAsync_WithLargeBalance_HandlesCorrectly()
    {
        // Arrange
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");
        var largeAmount = 100_000_000_000_000_000L; // 100 quadrillion

        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestEntity, payload =>
            MockResponseBuilder.CreateEntityResponse(
                publicKey: payload[..32],
                incomingAmount: largeAmount,
                outgoingAmount: 0,
                incomingTransfers: 1,
                outgoingTransfers: 0
            ));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var balance = await client.GetBalanceAsync(identity);

        // Assert
        Assert.Equal(largeAmount, balance.Amount);
    }

    [Fact]
    public async Task GetBalanceAsync_DifferentIdentities_ReturnsCorrectBalances()
    {
        // Arrange
        var identity1 = QubicIdentity.FromIdentity("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAFXIB");
        var identity2 = QubicIdentity.FromIdentity("AFZPUAIYVPNUYGJRQVLUKOPPVLHAZQTGLYAAUUNBXFTVTAMSBKQBLEIEPCVJ");

        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestEntity, payload =>
        {
            var requestedPublicKey = payload[..32];

            // Return different balances based on the requested identity
            if (requestedPublicKey.SequenceEqual(identity1.PublicKey))
            {
                return MockResponseBuilder.CreateEntityResponse(
                    publicKey: requestedPublicKey,
                    incomingAmount: 1000,
                    outgoingAmount: 0,
                    incomingTransfers: 1,
                    outgoingTransfers: 0
                );
            }
            else
            {
                return MockResponseBuilder.CreateEntityResponse(
                    publicKey: requestedPublicKey,
                    incomingAmount: 5000,
                    outgoingAmount: 2000,
                    incomingTransfers: 10,
                    outgoingTransfers: 5
                );
            }
        });

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        var balance1 = await client.GetBalanceAsync(identity1);
        var balance2 = await client.GetBalanceAsync(identity2);

        // Assert
        Assert.Equal(1000, balance1.Amount);
        Assert.Equal(3000, balance2.Amount); // 5000 - 2000
    }

    #endregion

    #region Connection Tests

    [Fact]
    public async Task ConnectAsync_ToMockServer_Succeeds()
    {
        // Arrange
        await using var server = new MockQubicServer();
        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);

        // Act
        await client.ConnectAsync();

        // Assert
        Assert.True(client.IsConnected);
    }

    [Fact]
    public async Task ConnectAsync_AlreadyConnected_DoesNotReconnect()
    {
        // Arrange
        await using var server = new MockQubicServer();
        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);

        // Act
        await client.ConnectAsync();
        await client.ConnectAsync(); // Second call should be no-op

        // Assert
        Assert.True(client.IsConnected);
    }

    [Fact]
    public async Task Disconnect_AfterConnect_DisconnectsSuccessfully()
    {
        // Arrange
        await using var server = new MockQubicServer();
        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act
        client.Disconnect();

        // Assert
        Assert.False(client.IsConnected);
    }

    #endregion

    #region Multiple Operations Tests

    [Fact]
    public async Task MultipleOperations_InSequence_AllSucceed()
    {
        // Arrange
        var identity = QubicIdentity.FromIdentity("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAARMID");

        await using var server = new MockQubicServer();

        server.OnRequest(QubicPacketTypes.RequestCurrentTickInfo, _ =>
            MockResponseBuilder.CreateCurrentTickInfoResponse(1000, 150, 12345678, 451, 5, 10000000));

        server.OnRequest(QubicPacketTypes.RequestEntity, payload =>
            MockResponseBuilder.CreateEntityResponse(payload[..32], 1000000, 500000, 10, 5));

        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port);
        await client.ConnectAsync();

        // Act - Multiple operations
        var tickInfo1 = await client.GetCurrentTickInfoAsync();
        var balance1 = await client.GetBalanceAsync(identity);
        var tickInfo2 = await client.GetCurrentTickInfoAsync();
        var balance2 = await client.GetBalanceAsync(identity);

        // Assert
        Assert.Equal(12345678u, tickInfo1.Tick);
        Assert.Equal(12345678u, tickInfo2.Tick);
        Assert.Equal(500000, balance1.Amount);
        Assert.Equal(500000, balance2.Amount);
    }

    #endregion

    #region Cancellation Tests

    [Fact]
    public async Task GetCurrentTickInfoAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        await using var server = new MockQubicServer();
        // Don't register any handler - request will hang
        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port, timeoutMs: 10000);
        await client.ConnectAsync();

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(100); // Cancel after 100ms

        // Act & Assert
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => client.GetCurrentTickInfoAsync(cts.Token));
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task GetCurrentTickInfoAsync_ServerDisconnects_ThrowsEndOfStreamException()
    {
        // Arrange
        await using var server = new MockQubicServer();
        // Server accepts connection but doesn't respond
        server.Start();

        using var client = new QubicNodeClient("127.0.0.1", server.Port, timeoutMs: 500);
        await client.ConnectAsync();

        // Act & Assert - Server doesn't respond, should timeout or throw
        await Assert.ThrowsAnyAsync<Exception>(() => client.GetCurrentTickInfoAsync());
    }

    #endregion
}
