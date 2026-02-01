using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Transfer record response.
/// </summary>
public sealed class BobTransferResponse
{
    [JsonPropertyName("txHash")]
    public string TransactionHash { get; set; } = string.Empty;

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;

    [JsonPropertyName("destination")]
    public string Destination { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0";

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("timestamp")]
    public long? Timestamp { get; set; }

    [JsonPropertyName("success")]
    public bool? Success { get; set; }

    public long AmountValue => long.TryParse(Amount, out var val) ? val : 0;
}
