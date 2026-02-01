using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Options for a tickStream subscription.
/// </summary>
public sealed class TickStreamOptions
{
    /// <summary>
    /// Tick number to start streaming from. The subscription will catch up from this tick.
    /// </summary>
    public uint? StartTick { get; set; }

    /// <summary>
    /// Whether to skip ticks with no matching transactions or logs.
    /// Default: false.
    /// </summary>
    public bool SkipEmptyTicks { get; set; }

    /// <summary>
    /// Whether to include transaction input data in the response.
    /// Default: true.
    /// </summary>
    public bool IncludeInputData { get; set; } = true;

    /// <summary>
    /// Optional transaction filters.
    /// </summary>
    public List<TxFilter>? TxFilters { get; set; }

    /// <summary>
    /// Optional log filters.
    /// </summary>
    public List<LogFilter>? LogFilters { get; set; }
}

/// <summary>
/// Filter for transactions in a tickStream subscription.
/// </summary>
public sealed class TxFilter
{
    [JsonPropertyName("from")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? From { get; set; }

    [JsonPropertyName("to")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? To { get; set; }

    [JsonPropertyName("minAmount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public long? MinAmount { get; set; }

    [JsonPropertyName("inputType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ushort? InputType { get; set; }
}

/// <summary>
/// Filter for logs in a tickStream subscription.
/// </summary>
public sealed class LogFilter
{
    [JsonPropertyName("scIndex")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? ScIndex { get; set; }

    [JsonPropertyName("logType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? LogType { get; set; }
}
