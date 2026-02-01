using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Sync status response from qubic_syncing.
/// </summary>
public sealed class SyncStatusResponse
{
    [JsonPropertyName("syncing")]
    public bool Syncing { get; set; }

    [JsonPropertyName("epoch")]
    public uint Epoch { get; set; }

    [JsonPropertyName("initialTick")]
    public uint InitialTick { get; set; }

    [JsonPropertyName("lastSeenNetworkTick")]
    public uint LastSeenNetworkTick { get; set; }

    [JsonPropertyName("currentFetchingTick")]
    public uint CurrentFetchingTick { get; set; }

    [JsonPropertyName("currentFetchingLogTick")]
    public uint CurrentFetchingLogTick { get; set; }

    [JsonPropertyName("currentVerifyLoggingTick")]
    public uint CurrentVerifyLoggingTick { get; set; }

    [JsonPropertyName("currentIndexingTick")]
    public uint CurrentIndexingTick { get; set; }
}
