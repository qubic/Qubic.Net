namespace Qubic.Bob;

/// <summary>
/// Tracks the health and sync state of a single Bob node.
/// </summary>
internal sealed class BobNodeState
{
    public required string BaseUrl { get; init; }
    public uint LastVerifyLoggingTick { get; set; }
    public uint LastSeenNetworkTick { get; set; }
    public DateTime LastHealthCheckUtc { get; set; }
    public TimeSpan Latency { get; set; }
    public int ConsecutiveFailures { get; set; }
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Returns the HTTP JSON-RPC URL for this node.
    /// </summary>
    public string GetHttpUrl(string rpcPath) => BaseUrl.TrimEnd('/') + rpcPath;

    /// <summary>
    /// Returns the WebSocket URL for this node.
    /// </summary>
    public string GetWebSocketUrl(string wsPath)
    {
        var baseUri = new Uri(BaseUrl);
        var scheme = baseUri.Scheme == "https" ? "wss" : "ws";
        return $"{scheme}://{baseUri.Host}:{baseUri.Port}{wsPath}";
    }
}
