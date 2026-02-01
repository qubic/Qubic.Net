using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// JSON-RPC 2.0 response structure.
/// </summary>
internal sealed class JsonRpcResponse<T>
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("result")]
    public T? Result { get; set; }

    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; set; }
}
