using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Transaction receipt response.
/// </summary>
public sealed class TransactionReceiptResponse
{
    [JsonPropertyName("txHash")]
    public string TxHash { get; set; } = string.Empty;

    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// Transaction hash — Bob may return it as "txHash" or "hash".
    /// </summary>
    [JsonIgnore]
    public string TransactionHash => !string.IsNullOrEmpty(TxHash) ? TxHash : Hash;

    /// <summary>
    /// Status as raw JsonElement — Bob may send as bool (true/false) or string ("true"/"false").
    /// </summary>
    [JsonPropertyName("status")]
    public JsonElement StatusRaw { get; set; }

    /// <summary>
    /// Whether the transaction was executed successfully.
    /// </summary>
    [JsonIgnore]
    public bool Status => StatusRaw.ValueKind switch
    {
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.String => bool.TryParse(StatusRaw.GetString(), out var val) && val,
        _ => false
    };

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }
}
