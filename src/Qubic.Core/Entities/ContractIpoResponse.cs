namespace Qubic.Core.Entities;

/// <summary>
/// Contract IPO response from the network.
/// </summary>
public sealed class ContractIpoResponse
{
    /// <summary>
    /// The contract index.
    /// </summary>
    public required uint ContractIndex { get; init; }

    /// <summary>
    /// The tick at which this data was retrieved.
    /// </summary>
    public required uint Tick { get; init; }

    /// <summary>
    /// The IPO data.
    /// </summary>
    public required ContractIpo Ipo { get; init; }
}
