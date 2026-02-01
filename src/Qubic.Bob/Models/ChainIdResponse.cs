using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Chain ID response.
/// </summary>
public sealed class ChainIdResponse
{
    [JsonPropertyName("chainId")]
    public string ChainId { get; set; } = string.Empty;

    [JsonPropertyName("chainIdDecimal")]
    public long ChainIdDecimal { get; set; }

    [JsonPropertyName("network")]
    public string Network { get; set; } = string.Empty;
}
