using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Log entry from smart contracts.
/// </summary>
public sealed class BobLogEntry
{
    [JsonPropertyName("logId")]
    public long LogId { get; set; }

    [JsonPropertyName("contractIndex")]
    public int ContractIndex { get; set; }

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("eventType")]
    public int EventType { get; set; }

    [JsonPropertyName("data")]
    public string? Data { get; set; }
}
