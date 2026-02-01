using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// JSON-RPC 2.0 notification received over WebSocket for subscriptions.
/// This differs from <see cref="JsonRpcResponse{T}"/> in that notifications
/// have a method field and nested params with subscription ID and result.
/// </summary>
internal sealed class JsonRpcMessage
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }

    [JsonPropertyName("method")]
    public string? Method { get; set; }

    [JsonPropertyName("params")]
    public SubscriptionNotificationParams? Params { get; set; }

    /// <summary>
    /// True if this is a subscription notification (method = "qubic_subscription").
    /// </summary>
    public bool IsNotification => Method == "qubic_subscription" && Params?.Result != null;

    /// <summary>
    /// True if this is a response to a request (has Id and no method).
    /// </summary>
    public bool IsResponse => Id.HasValue && Method is null;
}

/// <summary>
/// Params of a subscription notification.
/// </summary>
internal sealed class SubscriptionNotificationParams
{
    [JsonPropertyName("subscription")]
    public string? Subscription { get; set; }

    [JsonPropertyName("result")]
    public JsonElement? Result { get; set; }
}
