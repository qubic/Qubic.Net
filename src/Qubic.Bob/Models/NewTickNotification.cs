using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Data received from a newTicks subscription notification.
/// </summary>
public sealed class NewTickNotification
{
    [JsonPropertyName("tickNumber")]
    public uint TickNumber { get; set; }

    [JsonPropertyName("epoch")]
    public uint Epoch { get; set; }

    [JsonPropertyName("timestamp")]
    public long Timestamp { get; set; }

    [JsonPropertyName("timestampISO")]
    public string? TimestampISO { get; set; }

    [JsonPropertyName("computorIndex")]
    public int ComputorIndex { get; set; }

    [JsonPropertyName("transactionCount")]
    public int TransactionCount { get; set; }
}
