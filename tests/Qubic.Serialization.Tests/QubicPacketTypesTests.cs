using Qubic.Serialization;

namespace Qubic.Serialization.Tests;

public class QubicPacketTypesTests
{
    [Fact]
    public void RequestTypes_HaveCorrectValues()
    {
        // Verify request type constants match Qubic protocol specification
        Assert.Equal(0, QubicPacketTypes.ExchangePublicPeers);
        Assert.Equal(1, QubicPacketTypes.BroadcastMessage);
        Assert.Equal(2, QubicPacketTypes.BroadcastComputors);
        Assert.Equal(3, QubicPacketTypes.BroadcastTick);
        Assert.Equal(8, QubicPacketTypes.BroadcastFutureTickData);
        Assert.Equal(11, QubicPacketTypes.RequestComputors);
        Assert.Equal(14, QubicPacketTypes.RequestQuorumTick);
        Assert.Equal(16, QubicPacketTypes.RequestTickData);
        Assert.Equal(24, QubicPacketTypes.BroadcastTransaction);
        Assert.Equal(27, QubicPacketTypes.RequestCurrentTickInfo);
        Assert.Equal(29, QubicPacketTypes.RequestTickTransactions);
        Assert.Equal(31, QubicPacketTypes.RequestEntity);
    }

    [Fact]
    public void ResponseTypes_HaveCorrectValues()
    {
        // Verify response type constants
        Assert.Equal(28, QubicPacketTypes.RespondCurrentTickInfo);
        Assert.Equal(32, QubicPacketTypes.RespondEntity);
        Assert.Equal(34, QubicPacketTypes.RespondContractIPO);
        Assert.Equal(37, QubicPacketTypes.RespondIssuedAssets);
        Assert.Equal(39, QubicPacketTypes.RespondOwnedAssets);
        Assert.Equal(41, QubicPacketTypes.RespondPossessedAssets);
    }

    [Fact]
    public void AssetRequestTypes_HaveCorrectValues()
    {
        Assert.Equal(33, QubicPacketTypes.RequestContractIPO);
        Assert.Equal(36, QubicPacketTypes.RequestIssuedAssets);
        Assert.Equal(38, QubicPacketTypes.RequestOwnedAssets);
        Assert.Equal(40, QubicPacketTypes.RequestPossessedAssets);
    }

    [Fact]
    public void RequestAndResponseTypes_AreConsecutive()
    {
        // Most request/response pairs have consecutive type values
        Assert.Equal(QubicPacketTypes.RequestCurrentTickInfo + 1, QubicPacketTypes.RespondCurrentTickInfo);
        Assert.Equal(QubicPacketTypes.RequestEntity + 1, QubicPacketTypes.RespondEntity);
        Assert.Equal(QubicPacketTypes.RequestContractIPO + 1, QubicPacketTypes.RespondContractIPO);
        Assert.Equal(QubicPacketTypes.RequestIssuedAssets + 1, QubicPacketTypes.RespondIssuedAssets);
        Assert.Equal(QubicPacketTypes.RequestOwnedAssets + 1, QubicPacketTypes.RespondOwnedAssets);
        Assert.Equal(QubicPacketTypes.RequestPossessedAssets + 1, QubicPacketTypes.RespondPossessedAssets);
    }

    [Fact]
    public void AllTypeValues_AreUnique()
    {
        var types = new[]
        {
            QubicPacketTypes.ExchangePublicPeers,
            QubicPacketTypes.BroadcastMessage,
            QubicPacketTypes.BroadcastComputors,
            QubicPacketTypes.BroadcastTick,
            QubicPacketTypes.BroadcastFutureTickData,
            QubicPacketTypes.RequestComputors,
            QubicPacketTypes.RequestQuorumTick,
            QubicPacketTypes.RequestTickData,
            QubicPacketTypes.BroadcastTransaction,
            QubicPacketTypes.RequestCurrentTickInfo,
            QubicPacketTypes.RespondCurrentTickInfo,
            QubicPacketTypes.RequestTickTransactions,
            QubicPacketTypes.RequestEntity,
            QubicPacketTypes.RespondEntity,
            QubicPacketTypes.RequestContractIPO,
            QubicPacketTypes.RespondContractIPO,
            QubicPacketTypes.RequestIssuedAssets,
            QubicPacketTypes.RespondIssuedAssets,
            QubicPacketTypes.RequestOwnedAssets,
            QubicPacketTypes.RespondOwnedAssets,
            QubicPacketTypes.RequestPossessedAssets,
            QubicPacketTypes.RespondPossessedAssets
        };

        var distinctCount = types.Distinct().Count();
        Assert.Equal(types.Length, distinctCount);
    }

    [Fact]
    public void AllTypeValues_FitInByte()
    {
        var types = new[]
        {
            QubicPacketTypes.ExchangePublicPeers,
            QubicPacketTypes.BroadcastMessage,
            QubicPacketTypes.BroadcastComputors,
            QubicPacketTypes.BroadcastTick,
            QubicPacketTypes.BroadcastFutureTickData,
            QubicPacketTypes.RequestComputors,
            QubicPacketTypes.RequestQuorumTick,
            QubicPacketTypes.RequestTickData,
            QubicPacketTypes.BroadcastTransaction,
            QubicPacketTypes.RequestCurrentTickInfo,
            QubicPacketTypes.RespondCurrentTickInfo,
            QubicPacketTypes.RequestTickTransactions,
            QubicPacketTypes.RequestEntity,
            QubicPacketTypes.RespondEntity,
            QubicPacketTypes.RequestContractIPO,
            QubicPacketTypes.RespondContractIPO,
            QubicPacketTypes.RequestIssuedAssets,
            QubicPacketTypes.RespondIssuedAssets,
            QubicPacketTypes.RequestOwnedAssets,
            QubicPacketTypes.RespondOwnedAssets,
            QubicPacketTypes.RequestPossessedAssets,
            QubicPacketTypes.RespondPossessedAssets
        };

        foreach (var type in types)
        {
            Assert.True(type >= 0 && type <= 255, $"Type {type} does not fit in a byte");
        }
    }
}
