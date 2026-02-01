using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// JSON-RPC 2.0 request structure.
/// </summary>
internal sealed class JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; set; } = 1;

    [JsonPropertyName("method")]
    public required string Method { get; init; }

    [JsonPropertyName("params")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public object? Params { get; init; }
}
