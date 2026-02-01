using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// JSON-RPC 2.0 error structure.
/// </summary>
public sealed class JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}
