using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Balance response from qubic_getBalance.
/// </summary>
public sealed class BobBalanceResponse
{
    [JsonPropertyName("balance")]
    public JsonElement Balance { get; set; }

    [JsonPropertyName("currentTick")]
    public ulong CurrentTick { get; set; }

    [JsonPropertyName("identity")]
    public string? Identity { get; set; }

    [JsonPropertyName("incomingAmount")]
    public JsonElement IncomingAmount { get; set; }

    [JsonPropertyName("outgoingAmount")]
    public JsonElement OutgoingAmount { get; set; }

    [JsonPropertyName("numberOfIncomingTransfers")]
    public uint NumberOfIncomingTransfers { get; set; }

    [JsonPropertyName("numberOfOutgoingTransfers")]
    public uint NumberOfOutgoingTransfers { get; set; }

    [JsonPropertyName("latestIncomingTransferTick")]
    public uint LatestIncomingTransferTick { get; set; }

    [JsonPropertyName("latestOutgoingTransferTick")]
    public uint LatestOutgoingTransferTick { get; set; }

    /// <summary>
    /// Parses a JsonElement that may be a string or number to ulong.
    /// </summary>
    private static ulong ParseJsonElement(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetUInt64(out var u)) return u;
            if (element.TryGetInt64(out var s)) return s >= 0 ? (ulong)s : 0;
            return 0;
        }
        if (element.ValueKind == JsonValueKind.String)
        {
            return ulong.TryParse(element.GetString(), out var val) ? val : 0;
        }
        return 0;
    }

    /// <summary>Gets Balance as ulong.</summary>
    public ulong GetBalance() => ParseJsonElement(Balance);

    /// <summary>Gets IncomingAmount as ulong.</summary>
    public ulong GetIncomingAmount() => ParseJsonElement(IncomingAmount);

    /// <summary>Gets OutgoingAmount as ulong.</summary>
    public ulong GetOutgoingAmount() => ParseJsonElement(OutgoingAmount);
}
