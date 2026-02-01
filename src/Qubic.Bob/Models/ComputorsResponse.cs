using System.Text.Json.Serialization;

namespace Qubic.Bob.Models;

/// <summary>
/// Response from qubic_getComputors containing the list of 676 computor identities for an epoch.
/// </summary>
public sealed class ComputorsResponse
{
    [JsonPropertyName("computors")]
    public List<string> Computors { get; set; } = new();
}
