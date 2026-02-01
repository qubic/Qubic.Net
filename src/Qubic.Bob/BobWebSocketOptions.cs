namespace Qubic.Bob;

/// <summary>
/// Configuration options for <see cref="BobWebSocketClient"/>.
/// </summary>
public sealed class BobWebSocketOptions
{
    /// <summary>
    /// Bob node base URLs (e.g., "https://bob01.qubic.li").
    /// The client derives both HTTP (/qubic) and WebSocket (/ws/qubic) endpoints from these.
    /// At least one URL must be provided.
    /// </summary>
    public required string[] Nodes { get; init; }

    /// <summary>
    /// Path for the HTTP JSON-RPC endpoint, used for health checks.
    /// Default: "/qubic"
    /// </summary>
    public string HttpRpcPath { get; init; } = "/qubic";

    /// <summary>
    /// Path for the WebSocket JSON-RPC endpoint.
    /// Default: "/ws/qubic"
    /// </summary>
    public string WebSocketPath { get; init; } = "/ws/qubic";

    /// <summary>
    /// Interval between health checks on all nodes.
    /// Default: 30 seconds.
    /// </summary>
    public TimeSpan HealthCheckInterval { get; init; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Number of ticks the active node must fall behind the best node before switching.
    /// Default: 100 ticks (~100 seconds).
    /// </summary>
    public uint SwitchThresholdTicks { get; init; } = 100;

    /// <summary>
    /// Number of consecutive health check failures before a node is marked unavailable.
    /// Default: 3.
    /// </summary>
    public int FailureThreshold { get; init; } = 3;

    /// <summary>
    /// Initial delay before reconnecting after a connection failure.
    /// Default: 5 seconds.
    /// </summary>
    public TimeSpan ReconnectDelay { get; init; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum delay between reconnection attempts (exponential backoff cap).
    /// Default: 60 seconds.
    /// </summary>
    public TimeSpan MaxReconnectDelay { get; init; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// Size of the bounded channel buffer for each subscription.
    /// Default: 10000.
    /// </summary>
    public int SubscriptionBufferSize { get; init; } = 10_000;

    /// <summary>
    /// Optional callback for connection lifecycle events (node switches, reconnections, health checks).
    /// Useful for logging without requiring a dependency on ILogger.
    /// </summary>
    public Action<BobConnectionEvent>? OnConnectionEvent { get; init; }
}
