using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

internal sealed class ScQueryResponse
{
    [JsonPropertyName("nonce")]
    public int Nonce { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }

    [JsonPropertyName("pending")]
    public bool? Pending { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
