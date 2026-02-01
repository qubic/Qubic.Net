using Qubic.Bob;
using Qubic.Bob.Models;

namespace Qubic.Bob.Tests;

public class BobSubscriptionTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        var sub = new BobSubscription<TickStreamNotification>(
            "tickStream",
            new object[] { "tickStream", new { startTick = 100 } },
            () => new object[] { "tickStream", new { startTick = 200 } },
            1000);

        Assert.Equal("tickStream", sub.SubscriptionType);
        Assert.Null(sub.ServerSubscriptionId);
        Assert.False(sub.CancellationToken.IsCancellationRequested);

        sub.Dispose();
    }

    [Fact]
    public void ServerSubscriptionId_CanBeSet()
    {
        var sub = new BobSubscription<NewTickNotification>(
            "newTicks", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        sub.ServerSubscriptionId = "qubic_sub_0";

        Assert.Equal("qubic_sub_0", sub.ServerSubscriptionId);

        sub.Dispose();
    }

    [Fact]
    public void OnDisconnected_ClearsServerSubscriptionId()
    {
        var sub = new BobSubscription<NewTickNotification>(
            "newTicks", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        sub.ServerSubscriptionId = "qubic_sub_0";
        sub.OnDisconnected();

        Assert.Null(sub.ServerSubscriptionId);

        sub.Dispose();
    }

    [Fact]
    public void Dispose_CancelsCancellationToken()
    {
        var sub = new BobSubscription<NewTickNotification>(
            "newTicks", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        Assert.False(sub.CancellationToken.IsCancellationRequested);

        sub.Dispose();

        Assert.True(sub.CancellationToken.IsCancellationRequested);
    }

    [Fact]
    public void Dispose_MultipleTimes_DoesNotThrow()
    {
        var sub = new BobSubscription<NewTickNotification>(
            "newTicks", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        sub.Dispose();
        sub.Dispose(); // Should not throw
    }

    [Fact]
    public async Task WriteAsync_AndEnumerate_ReceivesData()
    {
        var sub = new BobSubscription<string>(
            "test", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        // Write items
        await sub.WriteAsync("hello", CancellationToken.None);
        await sub.WriteAsync("world", CancellationToken.None);

        // Read items
        var items = new List<string>();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await foreach (var item in sub.WithCancellation(cts.Token))
        {
            items.Add(item);
            if (items.Count == 2) break;
        }

        Assert.Equal(2, items.Count);
        Assert.Equal("hello", items[0]);
        Assert.Equal("world", items[1]);

        sub.Dispose();
    }

    [Fact]
    public async Task Enumeration_CompletesOnDispose()
    {
        var sub = new BobSubscription<string>(
            "test", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        await sub.WriteAsync("item1", CancellationToken.None);

        // Dispose after a short delay to end enumeration
        _ = Task.Run(async () =>
        {
            await Task.Delay(100);
            sub.Dispose();
        });

        var items = new List<string>();
        try
        {
            await foreach (var item in sub)
            {
                items.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected — dispose cancels the token
        }

        Assert.Contains("item1", items);
    }

    [Fact]
    public async Task Enumeration_CancelledByCancellationToken()
    {
        var sub = new BobSubscription<string>(
            "test", Array.Empty<object>(), () => Array.Empty<object>(), 100);

        await sub.WriteAsync("item1", CancellationToken.None);

        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(100));

        var items = new List<string>();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await foreach (var item in sub.WithCancellation(cts.Token))
            {
                items.Add(item);
            }
        });

        Assert.Contains("item1", items);

        sub.Dispose();
    }

    [Fact]
    public void ResubscribeParamsFactory_ReturnsUpdatedParams()
    {
        var callCount = 0;
        var sub = new BobSubscription<string>(
            "tickStream",
            new object[] { "tickStream" },
            () =>
            {
                callCount++;
                return new object[] { "tickStream", new { startTick = callCount * 100 } };
            },
            100);

        var params1 = sub.ResubscribeParamsFactory();
        var params2 = sub.ResubscribeParamsFactory();

        Assert.Equal(2, callCount);
        // Factory is called each time, so it can return updated state
        Assert.NotSame(params1, params2);

        sub.Dispose();
    }

    [Fact]
    public void IBobSubscription_Interface_ExposesAllMembers()
    {
        IBobSubscription sub = new BobSubscription<string>(
            "test", new object[] { "a" }, () => new object[] { "b" }, 100);

        Assert.Equal("test", sub.SubscriptionType);
        Assert.Single(sub.OriginalParams);
        Assert.NotNull(sub.ResubscribeParamsFactory);
        Assert.Null(sub.ServerSubscriptionId);
        Assert.False(sub.CancellationToken.IsCancellationRequested);

        sub.ServerSubscriptionId = "sub_1";
        Assert.Equal("sub_1", sub.ServerSubscriptionId);

        sub.OnDisconnected();
        Assert.Null(sub.ServerSubscriptionId);

        sub.Dispose();
        Assert.True(sub.CancellationToken.IsCancellationRequested);
    }
}
