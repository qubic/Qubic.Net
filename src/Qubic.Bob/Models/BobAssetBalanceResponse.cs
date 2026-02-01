using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Asset balance response.
/// </summary>
public sealed class BobAssetBalanceResponse
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;

    [JsonPropertyName("assetName")]
    public string AssetName { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = "0";

    public long AmountValue => long.TryParse(Amount, out var val) ? val : 0;
}
