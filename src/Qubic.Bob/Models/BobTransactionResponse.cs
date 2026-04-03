using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Transaction response.
/// </summary>
public sealed class BobTransactionResponse
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

    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    /// <summary>
    /// Source address — Bob may return it as "sourceId" or "from".
    /// </summary>
    [JsonIgnore]
    public string SourceAddress => !string.IsNullOrEmpty(SourceId) ? SourceId : From;

    [JsonPropertyName("destId")]
    public string DestId { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    /// <summary>
    /// Destination address — Bob may return it as "destId" or "to".
    /// </summary>
    [JsonIgnore]
    public string DestAddress => !string.IsNullOrEmpty(DestId) ? DestId : To;

    /// <summary>
    /// Amount as a raw JsonElement — Bob may send this as a string or a number.
    /// </summary>
    [JsonPropertyName("amount")]
    public JsonElement Amount { get; set; }

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("inputType")]
    public int InputType { get; set; }

    [JsonPropertyName("inputSize")]
    public int InputSize { get; set; }

    [JsonPropertyName("inputData")]
    public string? InputData { get; set; }

    public long AmountValue => Amount.ValueKind switch
    {
        JsonValueKind.String => long.TryParse(Amount.GetString(), out var val) ? val : 0,
        JsonValueKind.Number => Amount.GetInt64(),
        _ => 0
    };
}
