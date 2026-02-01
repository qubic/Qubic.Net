using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Transaction receipt response.
/// </summary>
public sealed class TransactionReceiptResponse
{
    [JsonPropertyName("txHash")]
    public string TransactionHash { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public bool Status { get; set; }

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }
}
