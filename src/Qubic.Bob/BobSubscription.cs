using System.Threading.Channels;

namespace Qubic.Bob;

/// <summary>
/// Internal interface for type-erased access to subscription state.
/// Used by <see cref="BobWebSocketClient"/> to manage subscriptions without knowing T.
/// </summary>
internal interface IBobSubscription : IDisposable
{
    string SubscriptionType { get; }
    object[] OriginalParams { get; }
    Func<object[]> ResubscribeParamsFactory { get; }
    string? ServerSubscriptionId { get; set; }
    CancellationToken CancellationToken { get; }
    void OnDisconnected();
}

/// <summary>
/// Represents an active WebSocket subscription that automatically tracks state
/// and resubscribes on reconnection. Exposes data as <see cref="IAsyncEnumerable{T}"/>.
/// </summary>
/// <typeparam name="T">The notification data type.</typeparam>
public sealed class BobSubscription<T> : IAsyncEnumerable<T>, IBobSubscription
{
    private readonly Channel<T> _channel;
    private readonly CancellationTokenSource _cts = new();
    private bool _disposed;

    /// <inheritdoc />
    public string SubscriptionType { get; }

    /// <inheritdoc />
    public object[] OriginalParams { get; }

    /// <inheritdoc />
    public Func<object[]> ResubscribeParamsFactory { get; set; }

    /// <inheritdoc />
    public string? ServerSubscriptionId { get; set; }

    /// <inheritdoc />
    public CancellationToken CancellationToken { get; }

    internal BobSubscription(
        string subscriptionType,
        object[] originalParams,
        Func<object[]> resubscribeParamsFactory,
        int bufferSize)
    {
        SubscriptionType = subscriptionType;
        OriginalParams = originalParams;
        ResubscribeParamsFactory = resubscribeParamsFactory;
        CancellationToken = _cts.Token;
        _channel = Channel.CreateBounded<T>(new BoundedChannelOptions(bufferSize)
        {
            FullMode = BoundedChannelFullMode.Wait
        });
    }

    /// <summary>
    /// Writes a notification into the subscription's channel.
    /// Called by <see cref="BobWebSocketClient"/> when a notification is received.
    /// </summary>
    internal async ValueTask WriteAsync(T item, CancellationToken cancellationToken)
    {
        await _channel.Writer.WriteAsync(item, cancellationToken);
    }

    /// <inheritdoc />
    public void OnDisconnected()
    {
        ServerSubscriptionId = null;
    }

    /// <summary>
    /// Enumerates notifications as they arrive. This is the primary consumer API.
    /// The enumeration pauses during reconnection and resumes automatically.
    /// </summary>
    public async IAsyncEnumerator<T> GetAsyncEnumerator(
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken, _cts.Token);

        await foreach (var item in _channel.Reader.ReadAllAsync(linkedCts.Token))
        {
            yield return item;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _cts.Cancel();
        _channel.Writer.TryComplete();
        _cts.Dispose();
    }
}
