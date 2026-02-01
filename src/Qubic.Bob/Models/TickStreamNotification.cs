using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Data received from a tickStream subscription notification.
/// </summary>
public sealed class TickStreamNotification
{
    [JsonPropertyName("epoch")]
    public uint Epoch { get; set; }

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("isCatchUp")]
    public bool IsCatchUp { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("txCountFiltered")]
    public uint TxCountFiltered { get; set; }

    [JsonPropertyName("txCountTotal")]
    public uint TxCountTotal { get; set; }

    [JsonPropertyName("logCountFiltered")]
    public uint LogCountFiltered { get; set; }

    [JsonPropertyName("logCountTotal")]
    public uint LogCountTotal { get; set; }

    [JsonPropertyName("transactions")]
    public List<TickStreamTransaction>? Transactions { get; set; }

    [JsonPropertyName("logs")]
    public List<TickStreamLog>? Logs { get; set; }
}

/// <summary>
/// Transaction within a tickStream notification.
/// </summary>
public sealed class TickStreamTransaction
{
    [JsonPropertyName("hash")]
    public string Hash { get; set; } = string.Empty;

    [JsonPropertyName("from")]
    public string From { get; set; } = string.Empty;

    [JsonPropertyName("to")]
    public string To { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public JsonElement Amount { get; set; }

    [JsonPropertyName("inputType")]
    public ushort InputType { get; set; }

    [JsonPropertyName("inputData")]
    public string? InputData { get; set; }

    [JsonPropertyName("executed")]
    public bool Executed { get; set; }

    [JsonPropertyName("logIdFrom")]
    public long LogIdFrom { get; set; }

    [JsonPropertyName("logIdLength")]
    public ushort LogIdLength { get; set; }

    /// <summary>
    /// Parses the amount, handling both string and number JSON representations.
    /// </summary>
    public long GetAmount()
    {
        if (Amount.ValueKind == JsonValueKind.String)
            return long.TryParse(Amount.GetString(), out var val) ? val : 0;
        if (Amount.ValueKind == JsonValueKind.Number)
            return Amount.GetInt64();
        return 0;
    }
}

/// <summary>
/// Log entry within a tickStream notification.
/// </summary>
public sealed class TickStreamLog
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("tick")]
    public uint Tick { get; set; }

    [JsonPropertyName("epoch")]
    public uint Epoch { get; set; }

    [JsonPropertyName("logId")]
    public long LogId { get; set; }

    [JsonPropertyName("type")]
    public byte LogType { get; set; }

    [JsonPropertyName("logTypename")]
    public string LogTypeName { get; set; } = string.Empty;

    [JsonPropertyName("logDigest")]
    public string? LogDigest { get; set; }

    [JsonPropertyName("bodySize")]
    public int BodySize { get; set; }

    [JsonPropertyName("timestamp")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("txHash")]
    public string? TxHash { get; set; }

    [JsonPropertyName("body")]
    public JsonElement? Body { get; set; }
}
