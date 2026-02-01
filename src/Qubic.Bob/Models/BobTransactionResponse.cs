using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Transaction response.
/// </summary>
public sealed class BobTransactionResponse
{
    [JsonPropertyName("txHash")]
    public string TransactionHash { get; set; } = string.Empty;

    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = string.Empty;

    [JsonPropertyName("destId")]
    public string DestId { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0";

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("inputType")]
    public int InputType { get; set; }

    [JsonPropertyName("inputSize")]
    public int InputSize { get; set; }

    public long AmountValue => long.TryParse(Amount, out var val) ? val : 0;
}
