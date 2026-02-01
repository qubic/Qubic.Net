using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Tick data response.
/// </summary>
public sealed class BobTickResponse
{
    [JsonPropertyName("tickNumber")]
    public uint TickNumber { get; set; }

    [JsonPropertyName("epoch")]
    public int Epoch { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("tickLeader")]
    public string? TickLeader { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}
